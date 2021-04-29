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
    bool attacking, shortAttacking, goingLeft, countingDown;
    public Transform floor;
    [SerializeField]
    float floorLevel, attackTimer, stuckTimer, health;
    Vector2 target;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        health = maxHealth;
        floorLevel = floor.position.y + 2;
        stuckTimer = 6;
        attackTimer = 6;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
        attackTimer -= Time.deltaTime;
        stuckTimer -= Time.deltaTime;
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
        if (attackTimer <= 0)
        {
            attackInit();
        }
        if (stuckTimer <= 0 && attackTimer > 0 && transform.position.y < returnPoint.position.y)
            MoveBack();
        else if (transform.position.y >= returnPoint.position.y && attackTimer > 0)
            countingDown = false;
        if (attacking)
            FloorAttack();
        else if (shortAttacking)
            ShortAttack();
        else
            Move();
    }

    private void Move()
    {
        if(stuckTimer < 0 && !attacking)
        {
            float step = moveSpeed  * Time.deltaTime;
            if (countingDown && attackTimer < 0)
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.position.x, returnPoint.position.y), step/4);
            else if(goingLeft)
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(leftBounds.position.x + 4, returnPoint.position.y), step);
            else
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(rightBounds.position.x - 1, returnPoint.position.y), step);
        }
    }

    void attackInit()
    {
        if (attacking || shortAttacking)
            return;
        Vector3 direction = (transform.position - player.position).normalized;
        float y = player.position.y - floorLevel;
        float scale = y / direction.y;

        target = player.position - direction * scale;
        target += playerRB.velocity * velocityMult;
        if (!countingDown && attackTimer <= 0)
        {
            countingDown = true;
            print("starting coroutine");
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
            attackTimer = attackCooldown;
            stuckTimer = stuckTime;
            Crumbling();
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
            attackTimer = attackCooldown*0.75f;
            stuckTimer = stuckTime*0.75f;
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
        }

        else
        {
            target = (Vector2)player.position + playerRB.velocity * velocityMult;
            shortAttacking = true;
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }

    }
    void Die()
    {
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
