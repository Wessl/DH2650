using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaspQueen : MonoBehaviour
{
    public static WaspQueen instance;
    public Transform player, returnPoint, leftBounds, rightBounds, attackPoint;
    public Rigidbody2D playerRB;
    public float moveSpeed, attackSpeed, attackCooldown, stuckTime, attackCountdown, attackRadius, attackDamage, maxHealth, velocityMult;
    public LayerMask playerLayer;
    public bool active, floorDestroyed;
    public Animator floorAnimator;
    bool attacking, shortAttacking, goingLeft, countingDown, movingBack, stuck;
    public Transform floor;
    public CapsuleCollider2D caps;
    [SerializeField]
    float floorLevel, attackTimer, stuckTimer, health;
    Vector2 target, targetDirection;
    public DragonBones.UnityArmatureComponent armature;
    float rot;
    public Material material;
    public ParticleSystem bloodSplat;
    public ParticleSystem boneSplat;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        health = maxHealth;
        floorLevel = floor.position.y + caps.size.y/2 + 1;
        stuckTimer = 6;
        attackTimer = 6;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        } else
            attackInit();
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
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, 15), Time.deltaTime * 50);
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(leftBounds.position.x + 4, returnPoint.position.y), step);
            }
            else
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, -15), Time.deltaTime * 50);
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(rightBounds.position.x - 1, returnPoint.position.y), step);
            }
        }
    }

    void attackInit()
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
            countingDown = true;
            print("starting coroutine");
            armature.animation.timeScale = 0.5f;
            armature.animation.Play("attackCountdown", 1);
            StartCoroutine(SetAttacking());
        }
    }

    void FloorAttack()
    {
        float step = attackSpeed * Time.deltaTime;
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
            attackTimer = Random.Range(attackCooldown * 0.75f, attackCooldown * 0.75f + 5);
            stuckTimer = stuckTime;
            CameraShake.instance.ShakeCamera(2.5f, 0.25f);
            shortAttacking = false;
        }
    }

    void CheckCollision()
    {
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
        if (hit)
        {
            if (hit.CompareTag("Player"))
            {
                Combat player = hit.GetComponent<Combat>();
                player.TakeDamage(attackDamage);
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
        material.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        material.color = Color.white;
    }

    void Die()
    {
        material.color = Color.white;
        Instantiate(bloodSplat, transform.position, Quaternion.identity);
        Instantiate(boneSplat, transform.position, Quaternion.identity);
        // Remove enemy gameObject from scene. 
        Destroy(gameObject);
    }

    public void Crumbling()
    {
        floorAnimator.SetTrigger("Crumbling");
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
