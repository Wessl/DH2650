using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Combat : MonoBehaviour
{
    public static Combat instance;
    public Animator animator;
    public Animator hitAnimation;
    public LayerMask enemyLayer, groundLayer;
    bool attacking, canAttack, upAttack, airAttack;
    Rigidbody2D rb;
    [SerializeField]
    GameObject slash;
    [SerializeField]
    Transform attackPoint;
    [SerializeField]
    float slash1radius, slash2radius;
    public float attackDamage, maxHealth, maxKi, pulledSlashCooldown, bulletDamage, damagedCooldown, attackLerp;
    [SerializeField]
    float health, ki;
    Animation slashAnimation;
    float attackMove, pulledSlashTimer, airAttackTimer;
    int attacked;
    Vector2 attackOrigin;
    Vector3 bulletDirection;
    string slashStr;
    float damageTimer, bulletDistance;
    RaycastHit2D[] bulletHits;
    private int attackMouseKeyCode;
    private GameObject youDiedPanel;
    public Transform geezer;
    private BarScript kiBar, healthBar;
    public LineRenderer line;
    private LayerMask bulletLayer;
    private SpriteRenderer SR;
    private List<GameObject> currentHighlights, oldHighlights;
    public bool hurting;

    void Awake()
    {
        currentHighlights = new List<GameObject>();
        oldHighlights = new List<GameObject>();
        bulletLayer = enemyLayer |= (1 << LayerMask.NameToLayer("PickUp"));
        line.SetVertexCount(2);
        kiBar = GameObject.FindWithTag("KiBar").GetComponent<BarScript>();
        healthBar = GameObject.FindWithTag("HealthBar").GetComponent<BarScript>();
        health = maxHealth;
        healthBar.UpdateBar(health / maxHealth);
        ki = maxKi;
        kiBar.UpdateBar(ki / maxKi);
        instance = this;
        rb = GetComponent<Rigidbody2D>();
        slashAnimation = slash.GetComponent<Animation>();
        SR = GetComponent<SpriteRenderer>();
        foreach (AnimationState anim in slashAnimation)
        {
            anim.speed = 0.75f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        youDiedPanel = GameObject.FindWithTag("GameCanvas").transform.Find("DeathPanel").gameObject;
        UpdateAttackButtonMapping();    // Set attack mouse key codes to the appropriate one according to settings
    }

    // Update is called once per frame
    void Update()
    {
        AttackInput();
        Heal();
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;
            if(damageTimer > 0.05)
            {
                FlashSprite();
            } else
            {
                SR.color = Color.white;
            }
        }
        if (pulledSlashTimer > 0)
            pulledSlashTimer -= Time.deltaTime;
        if (transform.position.y < -20)
            Die();
        BulletTime();
        if (airAttackTimer > 0)
            airAttackTimer -= Time.deltaTime;
    }


    private void FixedUpdate()
    {
        AttackMovement();
    }
    private void Heal()
    {
        if (health == maxHealth)
            return;
        if(ki >= 25 && Input.GetKeyDown(KeyCode.F))
        {
            UpdateHealth(50);
            UpdateKi(-25);
        }
    }
    public void AttackInput()
    {
        if (Input.GetMouseButtonDown(attackMouseKeyCode) && !TimeController.Instance.slowedTime)
        {
            if (PlayerMovement.instance.grounded)
            {
                if (Input.GetKey("up") || Input.GetKey("w"))
                {
                    if (airAttackTimer > 0 || hurting)
                        return;
                    upAttack = true;
                    animator.SetTrigger("UpAttack");
                    rb.velocity = new Vector2(rb.velocity.x/2, rb.velocity.y);
                    slashStr = "upslash";
                    airAttackTimer = 0.5f;
                    ExecuteAttack();
                }
                else
                {
                    attacking = true;
                    canAttack = false;
                }
            } else if (airAttackTimer <= 0 && !PlayerMovement.instance.touchingWall && !hurting)
            {
                PlayerMovement.instance.stickTimer = 0;
                airAttackTimer = 0.5f;
                //slash.transform.localRotation = Quaternion.Euler(180, 0, 180);
                if (Input.GetKey("up") || Input.GetKey("w"))
                {
                    slashStr = "upslashair";
                    animator.SetTrigger("UpAttackAir");
                } else
                {
                    slashStr = "airslash";
                    animator.SetTrigger("AirAttack");
                }
                ExecuteAttack();
            }
        }
    }
    public void SwitchAttackStatus()
    {
        canAttack = !canAttack;
    }

    public bool IsAttacking()
    {
        return attacking;
    }

    public void SetAttack(bool atk)
    {
            attacking = atk;
    }

    public void Attack(string name, float moveAmount)
    {
        slashStr = name;
        attackMove = PlayerMovement.instance.Orientation * moveAmount;
        attackOrigin = attackPoint.position;
        ExecuteAttack();
    }

    void ExecuteAttack()
    {
        float distance = attackPoint.position.x - attackOrigin.x;
        float damage = 0;
        Vector2 point = new Vector2(0, 0);
        float radius = 0;
        Vector2 size = new Vector2(0, 0);
        Vector2 direction = new Vector2(-PlayerMovement.instance.Orientation, 0);
        // play slash FX
        if(!slashStr.Equals("stab"))
            slash.GetComponent<Animation>().Play(slashStr);
        Vector2 slash1origin = attackOrigin + new Vector2(0.3f * PlayerMovement.instance.Orientation, -0.3f);
        Vector2 slash1point = (Vector2)attackPoint.position + new Vector2(0.3f * PlayerMovement.instance.Orientation, -0.3f);
        float distance1 = slash1point.x - slash1origin.x;

        // assign damage and attackpoint
        switch (slashStr)
        {
            case "altslash1":
                AudioManager.Instance.Play("Sword Swing 1");
                damage = attackDamage;
                point = slash1point;
                radius = slash1radius;
                //size = new Vector2(Mathf.Abs(distance1) + slash1radius * 2, slash1radius*2);
                LowGeezer(true);
                break;
            case "slash2":
                AudioManager.Instance.Play("Sword Swing 2");
                damage = attackDamage;
                point = attackPoint.position;
                radius = slash2radius;
                //size = new Vector2(Mathf.Abs(distance) + slash1radius * 2, slash2radius*2);
                LowGeezer(false);
                break;
            case "slash1":
            case "normalslash":
                AudioManager.Instance.Play("Sword Swing 1");
                damage = attackDamage;
                point = slash1point;
                radius = slash1radius;
                //size = new Vector2(Mathf.Abs(distance1) + slash1radius * 2, slash1radius*2);
                break;
            case "idleslash":
            case "runningslash":
            case "airslash":
                AudioManager.Instance.Play("Sword Swing 1");
                damage = attackDamage;
                point = slash1point;
                radius = slash1radius;
                //size = new Vector2(Mathf.Abs(distance1) + slash1radius*2, slash1radius*2);
                LowGeezer(true);
                break;
            case "rotatingslash":
            case "upslash":
            case "upslashair":
                AudioManager.Instance.Play("Sword Swing Air");
                damage = attackDamage*1.2f;
                point = transform.position + new Vector3(-0.7f, 1.5f, 0);
                radius = 2;
                break;
            default:
                print("Not a valid slash name");
                break;
        }
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius, enemyLayer);
        //hits = Physics2D.OverlapCapsuleAll(point, size, CapsuleDirection2D.Horizontal, 0, enemyLayer);
        //RaycastHit2D[] hitCollider = Physics2D.CircleCastAll(point, radius, direction, distance, enemyLayer);
        foreach (Collider2D target in hits)
        {
            print("enemy hit");
            AudioManager.Instance.Play("Sword Impact");
            //StartCoroutine(HitSleep(0.02f));
            string tag = target.tag;
            Damage(target.gameObject, tag, damage);
        }
    }

    public void UpdateKi(float value)
    {
        ki = Mathf.Clamp(ki + value, 0, maxKi);
        kiBar.UpdateBar(ki / maxKi);
    }

    public void UpdateHealth(float value)
    {
        health = Mathf.Clamp(health + value, 0, maxHealth);
        healthBar.UpdateBar(health / maxHealth);
    }

    private void Damage(GameObject target, string tag, float damage)
    {
        switch (tag)
        {
            case "GroundEnemy":
            case "FlyingEnemy":
                Enemy enemy = target.GetComponent<Enemy>();
                if (enemy.GetComponent<Animator>().GetBool("PulledEffect"))
                    damage *= 2;
                enemy.TakeDamage(damage);
                break;
            case "SnakeBoss":
                if (target.name.Equals("SnakeBoss"))
                    target.GetComponent<SnakeBoss>().TakeDamage(damage);
                else
                    GameObject.Find("SnakeBoss").GetComponent<SnakeBoss>().TakeDamage(damage);
                break;
            case "Larva":
                target.GetComponent<Larva>().TakeDamage();
                break;
            case "NewGroundEnemy":
            case "ToadGrunt":
                target.GetComponent<Enemy>().TakeDamage(damage);
                break;
            case "WaspQueen":
                print("waspqueen hit");
                target.GetComponent<WaspQueen>().TakeDamage(damage);
                break;
            case "StationaryShootingEnemy":
                Debug.Log("enemy hit is " + target.name);
                target.GetComponentInParent<ShooterBoye>().TakeDamage(damage);
                break;
            case "Projectile":
                target.GetComponent<Projectile>().Deflect();
                break;
            case "PickUp":
                target.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                break;
            default:
                print("Hit a new tag??? " + tag);
                break;

        }
    }
    /*
    public void PulledSlash(GameObject target, string tag, Vector2 position)
    {
        if (pulledSlashTimer > 0)
            return;
        Vector2 origin = transform.position;
        Vector2 direction = position - origin;
        transform.position = position;
        rb.velocity = direction.normalized * 50;
        PlayerMovement.instance.RetractTongue();
        Damage(target, tag, 50);
        animator.Play("Pulled slash");
        slash.GetComponent<Animation>().Play("runningslash");
        float dist = direction.magnitude;
        direction = direction.normalized;
        float rot = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (direction.x < 0)
        {
            transform.localRotation = Quaternion.Euler(0, 180, 180-rot);
        }
        else
            transform.localRotation = Quaternion.Euler(0, 0, rot);
        for (int i=0;i<dist;i++)
        {
            var afterImage = AfterImagePool.Instance.GetFromPool((int)Mathf.Sign(direction.x));
            afterImage.GetComponent<AfterImage>().PulledInit(0.8f*(i+1), 0.1f, position - direction * i);
        }
        PlayerMovement.instance.pulledSlash = 0.5f;
        pulledSlashTimer = pulledSlashCooldown;
    }*/

    void BulletTime()
    {
        if (!TimeController.Instance.slowedTime)
        {
            if (ki >= 50 && Input.GetKeyDown(KeyCode.LeftControl))
            {
                TimeController.Instance.SlowdownTime();
            }
        }
        else if (!TimeController.Instance.bulletSlashing)
        {
            currentHighlights.Clear();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 path = Vector2.ClampMagnitude(mousePos - transform.position, 20);
            float distance = path.magnitude;
            Vector3 direction = path.normalized;
            RaycastHit2D ground = Physics2D.Raycast(transform.position, direction, distance, groundLayer);
            if (ground)
            {
                float groundDistance = Vector2.Distance(ground.point, transform.position);
                path = Vector2.ClampMagnitude(path, groundDistance);
                distance = groundDistance;
            }
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, distance, bulletLayer);
            if (hits.Length > 0)
            {
                line.SetColors(Color.red, Color.red);
                foreach (RaycastHit2D hit in hits)
                {
                    GameObject target = hit.collider.gameObject;
                    currentHighlights.Add(target);
                    if (!oldHighlights.Contains(target))
                        oldHighlights.Add(target);
                    if (target.CompareTag("WaspQueen"))
                    {
                        WaspQueen.instance.material.color = new Color(255, 0, 0);
                    }
                    else
                    {
                        target.GetComponentInChildren<SpriteRenderer>().color = new Color(255, 0, 0);
                    }
                }
            }
            else
                line.SetColors(Color.white, Color.white);

            ResetHighlights();

            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position + path);

            if (Input.GetMouseButtonDown(attackMouseKeyCode))
            {
                CheckBulletSlash(line.GetPosition(0), direction, distance);
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
                TimeController.Instance.StopSlowdown(false);
        }
    }

    public void CheckBulletSlash(Vector2 origin, Vector3 direction, float distance)
    {
        bulletHits = Physics2D.RaycastAll(origin, direction, distance, bulletLayer);
        bulletDirection = direction;
        bulletDistance = distance;
        if (bulletHits.Length > 0)
        {
            if (Mathf.Sign(direction.x) != PlayerMovement.instance.Orientation)
                PlayerMovement.instance.Flip();
            animator.SetFloat("SlowdownFactor", 1 / TimeController.Instance.slowdownFactor);
            TimeController.Instance.slowdownTimer = 5;
            animator.Play("Bullet Time");
            AudioManager.Instance.Play("Bullet Time Init");
        }
    }

    public void BulletSlash()
    {
        transform.position += bulletDirection*bulletDistance;
        rb.velocity = new Vector2(0, 0);
        damageTimer = 0.01f;
        TimeController.Instance.StopSlowdown(true);
        foreach (RaycastHit2D hit in bulletHits)
        {
            if(hit.collider.CompareTag("WaspQueen"))
                WaspQueen.instance.SetOriginalColor();
            else
                hit.collider.gameObject.GetComponentInChildren<SpriteRenderer>().color = new Color(255, 255, 255);
            AudioManager.Instance.Play("Bullet Hit");
            Collider2D target = hit.collider;
            string tag = target.tag;
            if (target.CompareTag("PickUp"))
            {
                UpdateKi(target.GetComponent<PickUp>().boost);
                SnackPool.Instance.AddToPool(target.gameObject);
            }
            else
                Damage(target.gameObject, tag, bulletDamage);
            HitAnimatorPool.Instance.GetFromPool(hit.point);
        }
    }

    public void ResetLine()
    {
        line.SetWidth(0.1f, 0.1f);
        line.SetPosition(0, new Vector3(0, 0, 0));
        line.SetPosition(1, new Vector3(0, 0, 0));

        currentHighlights.Clear();
        ResetHighlights();
    }

    public void LowGeezer(bool low)
    {
        if (low)
            geezer.localPosition = new Vector2(0.5f, 0.6f);
        else
            geezer.localPosition = new Vector2(-0.1f, 1.1f);
            
    }

    public void HideGeezer(bool hide)
    {
        geezer.GetComponent<SpriteRenderer>().enabled = !hide;
    }

    public void AttackMovement()
    {
        if (attacked > 1)
        {
            // add very fast movement for one frame
            rb.velocity += new Vector2(attackMove, 0);
            attacked--;
            damageTimer = 0.025f;
        }
        else if (attacked > 0)
        {
            // stop movement
            rb.velocity = new Vector2(0, 0);
            ExecuteAttack();
            attacked--;
        }
    }

    public void TakeDamage(float damage)
    {
        if (damageTimer > 0)
            return;
        animator.SetTrigger("Hurt");
        hurting = true;
        animator.SetBool("LockedMovement", false);
        animator.SetBool("LockedDirection", false);
        UpdateHealth(-damage);
        damageTimer = damagedCooldown;
        //StartCoroutine(HitSleep(0.02f));
        CameraShake.instance.ShakeCamera(2.5f, 0.1f);
        print("DAMAGED: " + damage);
        if (health <= 0)
            Die();
    }

    public void Transition()
    {
        //rb.position += new Vector2(0.0025f*playerMovement.Orientation, 0);
    }

    void Die()
    {
        rb.gameObject.SetActive(false);
        // Save the amount of times you died to PlayerPrefs, increment old value by 1
        PlayerPrefs.SetInt("deathAmount", PlayerPrefs.GetInt("deathAmount") + 1);
        // Show the panel
        youDiedPanel.SetActive(true);
        // Update text showing you died and how many deaths you have so far
        var deaths = PlayerPrefs.GetInt("deathAmount");
        var deathDigitEnd = (deaths % 10);
        var ordinalNumberSuffix = "";
        switch (deathDigitEnd)
        {
            case 1:
                ordinalNumberSuffix = "ST";
                break;
            case 2:
                ordinalNumberSuffix = "ND";
                break;
            case 3:
                ordinalNumberSuffix = "RD";
                break;
            default:
                ordinalNumberSuffix = "TH";
                break;
        }
        // Edge cases are 11, 12 and 13
        if (deaths == 11 || deaths == 12 || deaths == 13)
        {
            ordinalNumberSuffix = "TH";
        }
        
        youDiedPanel.GetComponentInChildren<Text>().text = "YOU DIED FOR THE " + deaths + ordinalNumberSuffix + " TIME";
    }

    void FlashSprite()
    {
        SR.color = Color.Lerp(Color.white, new Color(0.2f, 0.2f, 0.2f), Mathf.PingPong((damagedCooldown-damageTimer)*(2.75f), 1));
    }

    public void UpdateAttackButtonMapping()
    {
        if (PlayerPrefs.GetInt("LeftMouseIsTongue") == 1)
        {
            attackMouseKeyCode = 1; // If mouse left click is tongue, then attack must be right mouse click
        }
        else
        {
            attackMouseKeyCode = 0; // If left mouse is not tongue, then attack must be left. 
        }
    }

    IEnumerator HitSleep(float time)
    {
        Time.timeScale = 0;
        float pauseEndTime = Time.realtimeSinceStartup + time;
        while (Time.realtimeSinceStartup < pauseEndTime)
        {
            yield return 0;
        }
        Time.timeScale = 1;
    }

    public void ResetHighlights()
    {
        for (int i = 0; i < oldHighlights.Count; i++)
        {
            if (!currentHighlights.Contains(oldHighlights[i]) && oldHighlights[i])
            {
                if (oldHighlights[i].CompareTag("WaspQueen"))
                    WaspQueen.instance.SetOriginalColor();
                else
                    oldHighlights[i].GetComponentInChildren<SpriteRenderer>().color = new Color(255, 255, 255);
                oldHighlights.RemoveAt(i);
                i--;
            }
        }
    }

    public void AttackSlow(float modifier)
    {
        rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0, rb.velocity.y), Time.deltaTime * attackLerp * modifier);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(new Vector3(0.3f, -0.3f, 0) + attackPoint.position, slash1radius);
        Gizmos.DrawWireSphere(attackPoint.position, slash2radius);
    }
}