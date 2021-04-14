using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyEnemyMovement : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    public float moveSpeed;

    public float detectionRange;
    public LayerMask playerLayer;

    private bool isAttacking;
    private Transform playerPos;
    public float turnSpeed;

    public float chaseTime;

    private void Start()
    {
        playerPos = transform;     // Default playerPos to the wasp's position
        isAttacking = false;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        InvokeRepeating("AttemptFrogDetection", 0f, 0.5f);
    }

    void FixedUpdate()
    {
        if (isAttacking)
        {
            GoTowardsFrog();
        }
        else
        {
            // If not chasing frog, reset velocity and rotation. 
            Quaternion targetRotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: Vector3.up);
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 1f);
        }
    }

    void AttemptFrogDetection()
    {
        Collider2D[] hitCollider = Physics2D.OverlapCircleAll(transform.position, detectionRange, playerLayer);
        if (hitCollider.Length > 0)
        {
            Debug.Log("player found beep boop");
            
            // Player frog detected. Initialize attack sequence. 
            if (!isAttacking)
            {
                playerPos = hitCollider[0].transform;
                isAttacking = true;
                animator.SetBool("isAttacking", isAttacking);
                StartCoroutine(ChasingTime());
            }
        }
    }

    void GoTowardsFrog()
    {
        // TODO
    }

    // Don't chase after the frog endlessly, get tired after a while and then stop until detecting again. 
    IEnumerator ChasingTime()
    {
        yield return new WaitForSeconds(chaseTime);
        isAttacking = false;
        animator.SetBool("isAttacking", isAttacking);
    }
    
}
