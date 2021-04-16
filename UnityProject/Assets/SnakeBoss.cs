using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Pathfinding;

/* TODO for snake:
 * Add player detection
 * Basic AI movement
 * Attacking & damage dealing
*/

public class SnakeBoss : MonoBehaviour
{
    public static SnakeBoss instance;
    public ParticleSystem bloodSplat;
    public ParticleSystem boneSplat;
    public Transform deathPSInstancePoint;
    public Transform target, headPoint, tailPoint;
    public Animator headSkeletonAnimator, headAnimator, tailSkeletonAnimator, tailAnimator;
    [Tooltip("Time it takes to accelerate/decelerate between min and max movespeed")]
    public float slowDownTime;
    public float maxHealth, weight;
    [SerializeField]
    float health, headSpeed;
    GameObject Head;
    public GameObject HeadInit;
    CircleCollider2D headCollider;
    public Transform rightHead, leftHead, backHead, rightTail, leftTail, backTail;
    public float headSwipeRadius, tailSwipeRadius;
    public float damage;
    float headTimer, tailTimer, combinedTimer, healthLoss, rotateTime;
    public float attackCooldown;
    public LayerMask playerLayer;
    public Transform rotationPoint;
    bool awake;
    public ButtonActionable linkedGameObject;

    void Start()
    {
        instance = this;
        Physics2D.IgnoreLayerCollision(12, 10);
        Physics2D.IgnoreLayerCollision(12,7);
        health = maxHealth;

    }

    private void Update()
    {
        if (!awake)
        {
            if (Vector2.Distance(PlayerMovement.instance.transform.position, transform.position) < 45)
            {
                headTimer = 7;
                combinedTimer = 7;
                tailTimer = 4;
                linkedGameObject.ButtonAction(true);
                awake = true;
            } else 
                return;
        }
        headTimer += Time.deltaTime;
        tailTimer += Time.deltaTime;
        combinedTimer += Time.deltaTime;

        if (combinedTimer > attackCooldown)
        {

            if (headTimer > attackCooldown * 2)
            {
                Collider2D leftArea = Physics2D.OverlapCircle(leftHead.position, headSwipeRadius, playerLayer);
                Collider2D rightArea = Physics2D.OverlapCircle(rightHead.position, headSwipeRadius, playerLayer);
                Collider2D[] hitCollider = Physics2D.OverlapCircleAll(rightHead.position, headSwipeRadius);
                if (leftArea != null)
                {
                    headAttack("Left Head");
                } else if (rightArea != null)
                {
                    headAttack("Right Head");
                }
            } else if (tailTimer > attackCooldown * 1.5f)
            {
                Collider2D leftArea = Physics2D.OverlapCircle(leftTail.position, tailSwipeRadius, playerLayer);
                Collider2D rightArea = Physics2D.OverlapCircle(rightTail.position, tailSwipeRadius, playerLayer);
                if (leftArea != null)
                {
                    tailAttack("Tail 2");
                }
                else if (rightArea != null)
                {
                    tailAttack("Tail 1");
                }
            }
        }

        if(healthLoss > 400)
        {
            Rotate();
        }
    }

    void headAttack(string head)
    {
        headAnimator.Play(head, -1, 0);
        headSkeletonAnimator.Play(head, -1, 0);
        headTimer = 0;
        combinedTimer = 0;
    }

    void tailAttack(string tail)
    {
        tailAnimator.Play(tail, -1, 0);
        tailSkeletonAnimator.Play(tail, -1, 0);
        tailTimer = 0;
        combinedTimer = 0;
    }

    void Strike()
    {
        Vector2 direction = (target.position - headPoint.position).normalized;
        Head = Instantiate(HeadInit, headPoint.position, Quaternion.identity);

        headCollider = Head.GetComponent<CircleCollider2D>();
        print(direction * headSpeed);
        Physics2D.IgnoreCollision(headCollider, GetComponent<PolygonCollider2D>());
        Head.GetComponent<Rigidbody2D>().AddForce(direction * headSpeed);
    }

    public void TakeDamage(float damage)
    {
        var ps = Instantiate(bloodSplat, transform.position, Quaternion.identity);
        health -= damage;
        healthLoss += damage;
        if (health <= 0)
        {
            Die();
        }
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


    void Rotate()
    {
        float rot = transform.localRotation.z;
        transform.RotateAround(rotationPoint.position, new UnityEngine.Vector3(0, 0, 1), 20 * Time.deltaTime);
        rotateTime += Time.deltaTime;
        if (rotateTime > 8)
        {
            healthLoss = 0;
            rotateTime = 0;
        }
    }
    public void AttackCollision(bool head) 
    {
        Transform trans = null;
        if (head)
        {
            trans = transform.Find("HeadSkeletonAnimator");
        } else
            trans = transform.Find("TailSkeletonAnimator");
        GetTheKids(trans);
    }

    void GetTheKids(Transform trans)
    {
        foreach (Transform child in trans)
        {
            bool hit = CheckChildCollision(child.position, child.GetComponent<CircleCollider2D>().radius, damage);
            if (hit)
                return;
            if (child.childCount > 0)
            {
                GetTheKids(child);
            }
        }
    }

    bool CheckChildCollision(Vector2 pos, float radius, float attackDamage)
    {
        Collider2D[] hitCollider = Physics2D.OverlapCircleAll(pos, radius);
        foreach (Collider2D target in hitCollider)
        {
            if (target.CompareTag("Player"))
            {
                Combat player = target.GetComponent<Combat>();
                player.TakeDamage(attackDamage);
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
       
        Gizmos.DrawWireSphere(rightHead.position, headSwipeRadius);

        Gizmos.DrawWireSphere(leftHead.position, headSwipeRadius);


        Gizmos.DrawWireSphere(rightTail.position, tailSwipeRadius);

        Gizmos.DrawWireSphere(leftTail.position, tailSwipeRadius);
        //Gizmos.DrawLine(ceilingCheck.position, new Vector3(ceilingCheck.position.x, ceilingCheck.position.y + ceilingCheckDist, ceilingCheck.position.z));
    }
}
