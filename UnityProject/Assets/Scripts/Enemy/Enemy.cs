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
    private Animator animator;
    private Rigidbody2D rb;
    private Material spriteMat;
    public float moveSpeed;
    public ParticleSystem bloodSplat;
    public ParticleSystem boneSplat;
    public Transform deathPSInstancePoint;
    public Transform target, attackPoint;
    [Tooltip("Time it takes to accelerate/decelerate between min and max movespeed")]
    public float slowDownTime;
    public float maxHealth, weight, attackRange, attackDamage, stopDistance;
    [SerializeField]
    float health;
    public LayerMask playerLayer;

    public float nextWaypointDist = 3;

    Path path;
    int currentWaypoint;
    Seeker seeker;

    Vector2 attackPointVector;

    void Start()
    {
        instance = this;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteMat = GetComponentInChildren<SpriteRenderer>().material;
        rb.freezeRotation = true;
        health = maxHealth;
        seeker = GetComponent<Seeker>();

        InvokeRepeating("UpdatePath", 0f, .5f);
        attackPointVector = rb.position - (Vector2)attackPoint.position;
        seeker.StartPath(rb.position, (Vector2)target.position + attackPointVector, PathComplete);
    }

    private void Update()
    {
        FollowPath();
    }

    void UpdatePath()
    {
        if(seeker.IsDone())
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

    void FollowPath()
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
                        rb.AddForce(movement * Time.deltaTime);
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

    private void OnTriggerStay2D(Collider2D other)
    {
        // If this detects a player, it will trigger an Attack animation.
        if (other.CompareTag("Player"))
        {
            animator.SetTrigger("attack");
        }
    }

    // Called from the animation behaviour state exit function
    public void AttackHit()
    {
        Collider2D[] hitCollider = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach (Collider2D target in hitCollider)
        {
            Debug.Log(target.tag);
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
        var ps = Instantiate(bloodSplat, transform.position, Quaternion.identity);
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
        spriteMat.SetColor("_Color", new Color(1,0.55f,0.55f,0.88f));   // Arbitrary values
        yield return new WaitForSeconds(0.07f);                                          // Arbitrary wait time, roughly four frames @60 fps
        // Reset to base value
        spriteMat.SetColor("_Color", new Color(1,1,1,0));               // Alpha 0 = default sprite color
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
    }
}
