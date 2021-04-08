using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
   
    void Start()
    {
        snakeAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        Move();
    }

    void Move()
    {
        rb.velocity = transform.right * moveSpeed * Time.deltaTime;
        if (rb.velocity.magnitude > 0)
        {
            snakeAnimator.SetBool("isMoving", true);
        }
    }
    
    void OnTriggerEnter2D(Collider2D col)
    {
        // Temporary solution? Future: Make unique tag for walls? Or other solution... 
        if (col.CompareTag("Ground"))
        {
            transform.Rotate(0, 180f, 0);
        }
    }
    
    
}
