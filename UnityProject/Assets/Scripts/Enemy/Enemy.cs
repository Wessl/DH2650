using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    public static Enemy instance;
    public Animator animator;
    private Rigidbody2D rb;
    private Material spriteMat;
    public float moveSpeed, lerp, jumpSpeed, jumpTimeDelay;
    public ParticleSystem bloodSplat;
    public ParticleSystem boneSplat;
    public Transform deathPSInstancePoint;
    public Transform attackPoint;
    private Transform target;
    [Tooltip("Time it takes to accelerate/decelerate between min and max movespeed")]
    public float slowDownTime;
    public float maxHealth, weight, attackRange, attackDamage, stopDistance;
    [SerializeField]
    float health;
    public LayerMask playerLayer;

    public float nextWaypointDist = 3;
    public float xMovement, groundCheckRadius, flyingLerp, engagementRange, attackCooldown, hitDelay;
    public LayerMask groundLayers;
    public bool inRange, grounded, engaged, canJump;
    public Transform groundCheck;
    public LayerMask playerAndGround;
    private float attackTimer, speed;

    Path path;
    int currentWaypoint;
    Seeker seeker;

    Vector2 attackPointVector;

    void Start()
    {
        target = GameObject.FindWithTag("Player").GetComponent<Transform>();
        instance = this; 
        if(animator == null)
            animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteMat = GetComponentInChildren<SpriteRenderer>().material;
        rb.freezeRotation = true;
        health = maxHealth;
        
        if(!CompareTag("NewGroundEnemy"))
            OldPathfindingSetup();
    }

    private void FixedUpdate()
    {
        if (CompareTag("NewGroundEnemy"))
        {
            IsGrounded();
            GroundMovement();
        } else if (engaged)
        {
            OldFollowPath();
        }
        FixDirection();
        float dist = (target.transform.position - transform.position).sqrMagnitude;
        if (!engaged && dist <= engagementRange)
            ScanForPlayer(dist);
        else if (dist > engagementRange)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0,0), Time.deltaTime * flyingLerp);
            engaged = false;
        }

        // If enemy is in melee range and attack isn't on cooldown, do an attack
        if(inRange && attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            animator.SetTrigger("attack");
        } else if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    // Called from animation behaviour state script
    // Either do hit detection immediately or after a delay
    public void HitDetection(bool delay)
    {
        if(delay)
            StartCoroutine(HitDelay());
        else
            AttackHit();
    }

    IEnumerator HitDelay()
    {
        yield return new WaitForSeconds(hitDelay);
        AttackHit();
    }

    void ScanForPlayer(float dist)
    {
        Vector2 dir = (target.transform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, Mathf.Sqrt(dist), playerAndGround);
        if (hit.collider.CompareTag("Player"))
        {
            engaged = true;
        }
    }

    void IsGrounded()
    {
        Collider2D ground = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);

        if (ground != null)
        {
            if (!grounded)
                StartCoroutine(CanJump());
            grounded = true;
        }
        else
        {
            canJump = false;
            grounded = false;
        }
    }

    // Enemy can jump when grounded after a small delay to prevent it from getting stuck in a jump-loop
    IEnumerator CanJump()
    {
        yield return new WaitForSeconds(0.1f);
        canJump = true;
    }

    void GroundMovement()
    {
        rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(xMovement * moveSpeed, rb.velocity.y), Time.deltaTime * lerp);
        speed = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("Speed", speed);

        // Set the speed of the animation based on the speed of the enemy
        if(speed > 0.5)
            animator.speed = speed / 2;
        else
            animator.speed = 1;
        Jump();
    }

    void Jump()
    {
        if (grounded && jumpSpeed > 0)
        {
            if (jumpTimeDelay > 0)
            {
                StartCoroutine(DelayJump(jumpSpeed));
            }
            else
            {
                rb.velocity = new Vector2(Math.Sign(rb.velocity.x) * jumpSpeed/3, jumpSpeed);
            }

            if (animator != null)
            {
                animator.SetTrigger("jump");
            }
        }
        jumpSpeed = 0;
    }

    IEnumerator DelayJump(float jumpSpeed)
    {
        yield return new WaitForSeconds(jumpTimeDelay);
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
    }

    // Fix the enemy direction either based on movement or on player position if within melee range
    void FixDirection()
    {
        if (!inRange)
        {
            if ((rb.velocity.x > 0 && transform.localScale.x < 0) ||
                        (rb.velocity.x < 0 && transform.localScale.x > 0))
            {
                transform.localScale *= new Vector2(-1, 1);
            }
        } else if (!(attackTimer > attackCooldown- hitDelay*2))
        {
            if ((transform.position.x < target.position.x && transform.localScale.x < 0) ||
                (transform.position.x > target.position.x && transform.localScale.x > 0))
            {
                transform.localScale *= new Vector2(-1, 1);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // If this detects a player, it will trigger an Attack animation.
        if (other.CompareTag("Player"))
        {
            animator.SetTrigger("attack");
        }
    }

    // Called from the HitDetection function
    public void AttackHit()
    {
        Collider2D[] hitCollider = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach (Collider2D target in hitCollider)
        {
            Debug.Log("enemy hit: " + target.tag);
            if (target.CompareTag("Player")) {
                Combat player = target.GetComponent<Combat>();
                player.TakeDamage(attackDamage);
            }
            //print(target.gameObject.layer);
            
            //Combat player = target.GetComponent<Combat>();
            //player.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        StartCoroutine(FlashSprite());
        
    }

    // Flash sprite white to indicate damage being taken
    IEnumerator FlashSprite()
    {
        Color color = spriteMat.GetColor("_Color");
        spriteMat.SetColor("_Color", new Color(1,0.55f,0.55f,0.88f));   // Arbitrary values
        yield return new WaitForSeconds(0.07f);                                          // Arbitrary wait time, roughly four frames @60 fps
        // Reset to base value
        spriteMat.SetColor("_Color", color);               // Alpha 0 = default sprite color
    }


    public void FlipRotation()
    {
        // Rotate instantly:
        transform.Rotate(0, 180f, 0);
    }

    void Die()
    {
        // Initialize death particle system(s)
        Instantiate(bloodSplat, deathPSInstancePoint.position, Quaternion.identity);
        Instantiate(boneSplat, deathPSInstancePoint.position, Quaternion.identity);
        // Remove enemy gameObject from scene. 
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        if(CompareTag("GroundEnemy"))
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    void UpdatePath()
    {   if (!engaged)
            return;
        if (seeker.IsDone())
            seeker.StartPath(rb.position, (Vector2)target.position + attackPointVector, PathComplete);
    }

    void PathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void OldPathfindingSetup()
    {
        seeker = GetComponent<Seeker>();
        InvokeRepeating("UpdatePath", 0f, .25f);
        attackPointVector = rb.position - (Vector2)attackPoint.position;
        seeker.StartPath(rb.position, (Vector2)target.position + attackPointVector, PathComplete);
    }

    void OldFollowPath()
    {
        if (path != null)
        {
            float distance = 1;
            if (tag.Equals("FlyingEnemy"))
                distance = Vector2.Distance(target.position, attackPoint.position);
            else if (tag.Equals("GroundEnemy"))
                distance = Mathf.Abs(target.position.x - attackPoint.position.x);
            if (0.5f < distance)
            {
                if (currentWaypoint >= path.vectorPath.Count)
                {
                    return;
                }

                Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
                Vector2 movement = direction * moveSpeed;
                float dist = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
                if (!animator.GetBool("PulledEffect"))
                {
                    if (tag.Equals("FlyingEnemy"))
                        rb.velocity = Vector2.Lerp(rb.velocity, movement, Time.deltaTime * flyingLerp);
                    else if (tag.Equals("GroundEnemy"))
                        rb.velocity = new Vector2(movement.x, rb.velocity.y);
                }
                if (dist < nextWaypointDist)
                {
                    currentWaypoint++;
                }
                if ((target.position.x > rb.position.x && transform.localScale.x < 0) ||
                    (target.position.x < rb.position.x && transform.localScale.x > 0))
                {
                    transform.localScale *= new Vector2(-1, 1);
                }
            }
            else
            {
                if (!animator.GetBool("PulledEffect"))
                {
                    rb.velocity = new Vector2(0, 0);
                }
            }


        }
    }
}
