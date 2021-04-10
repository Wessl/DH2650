using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

/* TODO for snake:
 * Add player detection
 * Basic AI movement
 * Attacking & damage dealing
*/

public class SnakeController : MonoBehaviour
{
    private Animator snakeAnimator;
    private Rigidbody2D rb;
    public float moveSpeed;
    public ParticleSystem bloodSplat;
    public ParticleSystem boneSplat;
    public Transform deathPSInstancePoint;
    [Tooltip("Time it takes to accelerate/decelerate between min and max movespeed")]
    public float slowDownTime;  
    
    [Range(1,1000)]
    public float health;

    void Start()
    {
        snakeAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    
    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (!snakeAnimator.GetCurrentAnimatorStateInfo(0).IsName("snake_attackanim"))
        {
            // If snake is not currently stuck in "attack" animation,
            rb.velocity = new Vector2(transform.right.x * moveSpeed * Time.deltaTime, rb.velocity.y);
            if (rb.velocity.magnitude > 0)
            {
                snakeAnimator.SetBool("isMoving", true);
            }
        }
        else
        {
            // If snake is stuck in attack animation, Lerp movespeed to 0. 
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, slowDownTime * Time.deltaTime);
        }
    }
    
    void OnTriggerStay2D(Collider2D col)
    {
        // Temporary solution? Future: Make unique tag for walls? Or other solution... 
        if (col.CompareTag("Ground"))
        {
            // Rotate instantly:
            transform.Rotate(0, 180f, 0);

        }
        // If this detects a player, it will trigger an Attack animation. (No damage at the moment)
        else if (col.CompareTag("Player"))
        {
            snakeAnimator.SetTrigger("attack");
        }
    }

    // Snake collides with something
    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If any of those things are a weapon, enemy has been hit. 
            if (contact.collider.CompareTag("Weapon"))          
            {
                var ps = Instantiate(bloodSplat, contact.point, Quaternion.identity);
                TakeDamage(contact.collider.gameObject);
                break;
            }
        }
    }

    void TakeDamage(GameObject weaponObject)
    {
        health -= weaponObject.GetComponent<Weapon>().Damage;
        if (health <= 0)
        {
            // Enemy has died. 
            // Initialize death particle system(s)
            Instantiate(bloodSplat, deathPSInstancePoint.position, Quaternion.identity);
            Instantiate(boneSplat, deathPSInstancePoint.position, Quaternion.identity);
            // Remove enemy gameObject from scene. 
            Destroy(gameObject);
        }
    }
}
