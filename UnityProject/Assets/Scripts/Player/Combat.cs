using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Combat : MonoBehaviour
{
    public static Combat instance;
    public Animator animator;
    public LayerMask enemyLayer;
    bool attacking, canAttack;
    Rigidbody2D rb;
    [SerializeField]
    GameObject slash;
    [SerializeField]
    Transform attackPoint;
    [SerializeField]
    float slash1height, slash2range;
    public LayerMask rayMask;
    public float attackDamage, maxHealth;
    [SerializeField]
    float health;
    Animation slashAnimation;
    float attackMove;
    int attacked;
    Vector2 attackOrigin;
    string slashStr;
    float damageTimer;
    private int attackMouseKeyCode;
    public GameObject youDiedPanel;
    public Transform geezer;

    void Awake()
    {
        health = maxHealth;
        instance = this;
        rb = GetComponent<Rigidbody2D>();
        rayMask = ~(1 << LayerMask.NameToLayer("Enemy"));
        slashAnimation = slash.GetComponent<Animation>();
        foreach (AnimationState anim in slashAnimation)
        {
            anim.speed = 0.5f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateAttackButtonMapping();    // Set attack mouse key codes to the appropriate one according to settings
    }

    // Update is called once per frame
    void Update()
    {
        Attack();
        damageTimer += Time.deltaTime;
        if (transform.position.y < -20)
            Die();
    }


    private void FixedUpdate()
    {
        AttackMovement();
    }

    public void Attack()
    {
        if (Input.GetMouseButtonDown(attackMouseKeyCode))
        {
            if (PlayerMovement.instance.grounded)
            {
                attacking = true;
                canAttack = false;
            } else if (!PlayerMovement.instance.touchingCeiling)
            {
                //slash.transform.localRotation = Quaternion.Euler(180, 0, 180);
                slashStr = "rotatingslash";
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
        if (moveAmount > 0)
            attacked = 2;
        else
            ExecuteAttack();
    }

    void ExecuteAttack()
    {
        float distance = Vector2.Distance(attackPoint.position, attackOrigin);
        float damage = 0;
        Vector2 point = new Vector2(0, 0);
        float radius = 0;
        Vector2 size = new Vector2(0, 0);
        Vector2 direction = new Vector2(-PlayerMovement.instance.Orientation, 0);
        // play slash FX
        slash.GetComponent<Animation>().Play(slashStr);
        Vector2 slash1origin = attackOrigin + new Vector2(0.3f, -0.3f);
        Vector2 slash1point = (Vector2)attackPoint.position + new Vector2(0.3f, -0.3f);

        // assign damage and attackpoint
        switch (slashStr)
        {
            case "altslash1":
                damage = attackDamage;
                point = slash1point - new Vector2(distance / 2, 0);
                radius = slash1height;
                size = new Vector2(distance, slash1height);
                LowGeezer(true);
                break;
            case "slash2":
                damage = attackDamage * 1.5f;
                point = (Vector2)attackPoint.position - new Vector2(distance / 2, 0);
                radius = slash2range;
                size = new Vector2(distance, slash2range);
                LowGeezer(false);
                break;
            case "slash1":
            case "normalslash":
                damage = attackDamage * 1.5f;
                point = slash1point - new Vector2(distance / 2, 0);
                radius = slash1height;
                size = new Vector2(distance, slash1height);
                break;
            case "idleslash":
            case "runningslash":
                damage = attackDamage * 1.5f;
                point = slash1point - new Vector2(distance/2, 0);
                radius = slash1height;
                size = new Vector2(distance, slash1height);
                LowGeezer(true);
                break;
            case "rotatingslash":
                damage = attackDamage;
                point = attackPoint.position;
                radius = slash2range;
                break;
            default:
                print("Not a valid slash name");
                break;
        }
        Collider2D[] hit = null;
        // raycast to start point of attack
        if (slashStr.Equals("rotatingslash"))
            hit = Physics2D.OverlapCircleAll(point, radius, enemyLayer);
        else
            hit = Physics2D.OverlapCapsuleAll(point, size, CapsuleDirection2D.Horizontal, 0, enemyLayer);
        //RaycastHit2D[] hitCollider = Physics2D.CircleCastAll(point, radius, direction, distance, enemyLayer);
        foreach (Collider2D target in hit)
        {
            print("enemy hit");
            string tag = target.tag;
            Enemy enemy;
            switch (tag)
            {
                case "GroundEnemy":
                case "FlyingEnemy":
                    enemy = target.GetComponent<Enemy>();
                    if (enemy.GetComponent<Animator>().GetBool("PulledEffect"))
                        damage *= 2;
                    enemy.TakeDamage(damage);
                    break;
                case "SnakeBoss":
                    if(target.name.Equals("SnakeBoss"))
                        target.GetComponent<SnakeBoss>().TakeDamage(damage);
                    else
                       GameObject.Find("SnakeBoss").GetComponent<SnakeBoss>().TakeDamage(damage);
                    break;
                case "Larva":
                    target.GetComponent<Larva>().TakeDamage();
                    break;
                case "NewGroundEnemy":
                    target.GetComponent<Enemy>().TakeDamage(damage);
                    break;
                case "WaspQueen":
                    print("waspqueen hit");
                    target.GetComponent<WaspQueen>().TakeDamage(damage);
                    break;
                default:
                    print("Hit a new tag??? " + tag);
                    break;

            }
        }
    }

    public void LowGeezer(bool low)
    {
        if (low)
            geezer.localPosition = new Vector2(0.5f, 0.6f);
        else
            geezer.localPosition = new Vector2(-0.1f, 1.1f);
            
    }

    public void AttackMovement()
    {
        if (attacked > 1)
        {
            // add very fast movement for one frame
            rb.velocity += new Vector2(attackMove, 0);
            attacked--;
        }
        else if (attacked > 0)
        {
            // stop movement
            rb.velocity = new Vector2(0, rb.velocity.y);
            ExecuteAttack();
            attacked--;
        }
    }

    public void TakeDamage(float damage)
    {
        if (damageTimer < 2)
            return;
        animator.SetTrigger("Hurt");
        health -= damage;
        damageTimer = 0;
        CameraShake.instance.ShakeCamera(2.5f, 0.1f);
        print("DAMAGED");
        if (health < 0)
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(new Vector3(0.3f, -0.3f, 0) + attackPoint.position, slash1height);
        Gizmos.DrawWireSphere(attackPoint.position, slash2range);
    }
}