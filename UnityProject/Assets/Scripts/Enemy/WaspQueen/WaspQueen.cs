using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaspQueen : MonoBehaviour
{
    public static WaspQueen instance;
    public Transform player, returnPoint, leftBounds, rightBounds, attackPoint, crownPoint, stinger;
    public Rigidbody2D playerRB;
    public float moveSpeed, attackSpeed, attackCooldown, stuckTime, attackCountdown, attackRadius, attackDamage, sideAttackDamage, maxHealth, velocityMult, temporaryBoost;
    public LayerMask playerLayer;
    public bool active, floorDestroyed;
    public Animator floorAnimator;
    bool attacking, shortAttacking, sideAttacking, goingLeft, countingDown, movingBack, stuck, followingRoute;
    public Transform floor, heightMarker;
    public CapsuleCollider2D caps;
    [SerializeField]
    float floorLevel, attackTimer, stuckTimer, health;
    Vector2 target, targetDirection; 
    public Vector3 crownSize;
    public float flashDuration = 0.08f;
    public DragonBones.UnityArmatureComponent armature;
    float rot;
    int phase;  
    public Material material;
    public ParticleSystem bloodSplat;
    public ParticleSystem boneSplat;
    [SerializeField]
    private Transform[] routes0, routes1, routes2, routes3;
    private Transform[][] paths;
    private Transform[] currentPath;
    Vector3 phase2pos;
    private int currentRoute = 0;
    int pathIndex;
    private float tParam = 0;
    private Shader whiteShader, defaultShader;
    string[] lines;
    public TextAsset textFile;
    public Sprite itemAvatar;
    Color originalColor;
    [SerializeField]
    private GameObject minionPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (textFile != null)
        {
            lines = textFile.text.Split('\n');
        }
        whiteShader = Shader.Find("GUI/Text Shader");
        defaultShader = material.shader;
        paths = new Transform[][] { routes0, routes1, routes2, routes3 };
        instance = this;
        health = maxHealth;
        floorLevel = floor.position.y + caps.size.y/2 + 2;
        stuckTimer = 6;
        attackTimer = 6;
        armature.animation.Play("flying");
        originalColor = material.color;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (stuckTimer > 0)
        {

            stuckTimer -= Time.deltaTime;
            if (stuckTimer <= 0 && attackTimer > 0 && transform.position.y < returnPoint.position.y && !movingBack)
            {
                armature.animation.Play("flying", -1);
                movingBack = true;
                stuck = false;
            }
            if (!armature.animation.isPlaying) {
                armature.animation.Play("stuck", 1);
                stuck = true;
            }
            if (stuck)
            {
                var dir = Quaternion.AngleAxis(rot, Vector3.forward) * Vector3.right;
                transform.position = Vector2.MoveTowards(transform.position, transform.position - dir * 50, 0.1f * Time.deltaTime);
            }
        }
        if(transform.position.x == leftBounds.position.x+4)
        {
            goingLeft = false;
        } else if (transform.position.x == rightBounds.position.x-1)
        {
            goingLeft = true;
        }
        if(followingRoute)
        {
            CheckSideCollision();
        }
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        else if (phase == 2)
        {
            phase2pos = transform.position;
            sideAttacking = true;
            phase = 3;
        } else if (phase == 0)
        {
            AttackInit();
        } 
        /*
        if(Input.GetKeyDown(KeyCode.F))
        {
            MinionAttack();
        }*/
    }

    private void FixedUpdate()
    {
        if (movingBack)
            MoveBack();
        else if (attackTimer > 0)
        {
            countingDown = false;
        }
        if (attacking)
            FloorAttack();
        else if (shortAttacking)
            ShortAttack();
        else if(sideAttacking)
        {
            if (!followingRoute)
            {
                if (phase == 4)
                {
                    StartCoroutine(FollowBezier());
                    return;
                }
                if (phase2pos.x < heightMarker.position.x)
                {
                    pathIndex = 0;
                    goingLeft = true;
                }
                else
                {
                    pathIndex = 2;
                    goingLeft = false;
                }
                currentPath = paths[pathIndex];
                Vector3 startPos = currentPath[currentRoute].GetChild(0).position;
                if (transform.position == startPos)
                {
                    if (player.position.y < heightMarker.position.y) 
                        currentPath = paths[++pathIndex];

                    if (pathIndex < 2)
                        SnackPool.Instance.GetFromPool(new Vector2(returnPoint.position.x - 20, returnPoint.position.y), 25);
                    else
                        SnackPool.Instance.GetFromPool(new Vector2(returnPoint.position.x + 20, returnPoint.position.y), 25);
                    phase = 4;
                    AudioManager.Instance.Play("Wasp Queen Attack 2");
                }
                else
                {
                    targetDirection = (startPos - transform.position).normalized;
                    rot = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
                    float localZ = transform.localRotation.z;
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, rot + 270),
                        Time.deltaTime * moveSpeed * (10 + Mathf.Abs(localZ - (rot + 90))));

                    transform.position = Vector2.MoveTowards(transform.position, startPos, Time.deltaTime * moveSpeed);
                }
            }
        }
        else
            Move();
    }


    private void Move()
    {
        if(stuckTimer < 0 && !attacking && !movingBack)
        {
            float step = moveSpeed  * Time.deltaTime;
            if (countingDown && attackTimer < 0)
            {
                targetDirection = (target - (Vector2)transform.position).normalized;
                rot = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
                float localZ = transform.localRotation.z;
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, rot + 90), Time.deltaTime * 3 * ( 10 + Mathf.Abs(localZ-(rot+90))));
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.position.x, returnPoint.position.y), step / 4);
            }
            else if (goingLeft)
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, 15), Time.deltaTime * 20);
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(leftBounds.position.x + 4, returnPoint.position.y), step);
            }
            else
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, -15), Time.deltaTime * 20);
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(rightBounds.position.x - 1, returnPoint.position.y), step);
            }
        }
    }

    void AttackInit()
    {
        if (attacking || shortAttacking)
            return;
        targetDirection = (transform.position - player.position).normalized;
        float y = player.position.y - floorLevel;
        float scale = y / targetDirection.y;
        target = (Vector2)player.position - targetDirection * scale - targetDirection * Mathf.Abs(targetDirection.x) * 2;
        target += playerRB.velocity * velocityMult;
        if (!countingDown && attackTimer <= 0)
        {
            AudioManager.Instance.Play("Wasp Queen Attack 1");
            countingDown = true;
            print("starting coroutine");
            armature.animation.timeScale = 0.5f;
            armature.animation.Play("attackCountdown", 1);
            StartCoroutine(SetAttacking());
        }
    }

    void FloorAttack()
    {
        float step = (attackSpeed + temporaryBoost) * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target, step);
        CheckCollision();
        if ((Vector2)transform.position == target)
        {
            armature.animation.Play("impact", 1);
            attackTimer = Random.Range(attackCooldown, attackCooldown+5);
            stuckTimer = stuckTime;
            Crumbling();
            CameraShake.instance.ShakeCamera(5,0.4f);
            attacking = false;
            temporaryBoost = 0;
            phase = 2;
            SnackPool.Instance.GetFromPool(new Vector2(returnPoint.position.x, returnPoint.position.y), 25);
        }
    }

    void ShortAttack()
    {
        float step = attackSpeed * 0.75f * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target, step);
        CheckCollision();
        if ((Vector2)transform.position == target)
        {
            armature.animation.Play("impact", 1);
            AudioManager.Instance.Play("Wasp Queen Impact");
            attackTimer = Random.Range(attackCooldown * 0.75f, attackCooldown * 0.75f + 5);
            stuckTimer = stuckTime;
            CameraShake.instance.ShakeCamera(2.5f, 0.25f);
            shortAttacking = false;
            temporaryBoost = 0;
            phase = 2;
            SnackPool.Instance.GetFromPool(new Vector2(returnPoint.position.x, returnPoint.position.y), 25);
        }
    }

    void CheckCollision()
    {
        Collider2D hit = Physics2D.OverlapCircle(stinger.position, attackRadius / 3, playerLayer);        
        if (hit)
        {
            if (hit.CompareTag("Player"))
            {
                Combat.instance.TakeDamage(attackDamage + temporaryBoost);
            }
        }
        else
        {
            hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);

            if (hit)
            {
                if (hit.CompareTag("Player"))
                {
                    Combat.instance.TakeDamage(attackDamage + temporaryBoost);
                }
            }
        }
    }

    void CheckSideCollision()
    {
        Collider2D hit = Physics2D.OverlapCapsule(crownPoint.position, crownSize, CapsuleDirection2D.Horizontal, rot + 270, playerLayer);
        if (hit)
        {
            if (hit.CompareTag("Player"))
            {
                Combat.instance.TakeDamage(attackDamage);
            }
        }
    }

    void MoveBack()
    {
        if(transform.position.y >= returnPoint.position.y)
        {
            movingBack = false;
            return;
        }
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime*50);
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, returnPoint.position.y), step);
    }
    IEnumerator SetAttacking()
    {
        yield return new WaitForSeconds(attackCountdown);
        print("attacking");
        phase = 1;
        if (target.x > leftBounds.position.x && target.x < rightBounds.position.x && !floorDestroyed)
        {
            target.y -= playerRB.velocity.y * velocityMult;
            attacking = true;
            armature.animation.timeScale = 1f;
            armature.animation.Play("attack", 1);
        }
        else
        {
            target = (Vector2)player.position + playerRB.velocity * velocityMult - targetDirection;
            shortAttacking = true;
            armature.animation.timeScale = 1f;
            armature.animation.Play("attack", 1);
        }
    }

    public void TakeDamage(float damage)
    {
        StartCoroutine(Flash());
        health -= damage;
        if (health <= 0)
        {
            Die();
        }

    }
    IEnumerator Flash()
    {
        material.shader = whiteShader;
        yield return new WaitForSeconds(flashDuration);
        material.shader = defaultShader;
    }

    void Die()
    {
        AudioManager.Instance.Stop("Boss 1 Theme");
        AudioManager.Instance.Play("Area 1 Theme");
        WaspQueenCamera.Instance.ResetCamera();
        material.shader = defaultShader;
        PlayerMovement.instance.dashLocked = false;
        TextBoxManager.Instance.PrintText(lines, itemAvatar);
        Instantiate(bloodSplat, transform.position, Quaternion.identity);
        Instantiate(boneSplat, transform.position, Quaternion.identity);
        // Remove enemy gameObject from scene. 
        Destroy(gameObject);
    }

    void MinionAttack()
    {
        var minion = Instantiate(minionPrefab,transform.position,new Quaternion(0,0,0,0));

    }

    public void Crumbling()
    {
        AudioManager.Instance.Play("Crumbling");
        floorAnimator.SetTrigger("Crumbling");
    }

    private IEnumerator FollowBezier()
    {
        followingRoute = true;
        Transform route = currentPath[currentRoute];
        Vector2 pos0 = route.GetChild(0).position;
        Vector2 pos1 = route.GetChild(1).position;
        Vector2 pos2 = route.GetChild(2).position;
        Vector2 pos3 = route.GetChild(3).position;

        while (tParam < 1)
        {
            float sideSpeed = moveSpeed / 20;
            tParam += Time.deltaTime * sideSpeed;

            Vector2 newPosition = Mathf.Pow(1 - tParam, 3) * pos0 +
                3 * tParam * Mathf.Pow(1 - tParam, 2) * pos1 +
                3 * (1 - tParam) * Mathf.Pow(tParam, 2) * pos2 +
                Mathf.Pow(tParam, 3) * pos3;
            yield return new WaitForEndOfFrame();

            targetDirection = (newPosition - (Vector2)transform.position).normalized;
            rot = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            float localZ = transform.localRotation.z;
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, rot + 270), 
                Time.deltaTime * 20 * sideSpeed * (10 + Mathf.Abs(localZ - (rot + 90))));

            transform.position = newPosition;
        }

        tParam = 0;
        currentRoute++;

        if (currentRoute >= currentPath.Length)
        {
            currentRoute = 0;
            sideAttacking = false;
            attackTimer = Random.Range(attackCooldown * 0.75f, attackCooldown * 0.75f + 5);
            phase = 0;
        }

        followingRoute = false;
    }

    public void SetOriginalColor()
    {
        material.color = originalColor;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        Gizmos.DrawWireSphere(stinger.position, attackRadius/3);
        Gizmos.DrawWireCube(crownPoint.position, crownSize);
    }
}
