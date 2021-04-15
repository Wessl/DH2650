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
    public ParticleSystem bloodSplat;
    public ParticleSystem boneSplat;
    public Transform deathPSInstancePoint;
    public Transform target, headPoint, tailPoint;
    [Tooltip("Time it takes to accelerate/decelerate between min and max movespeed")]
    public float slowDownTime;
    [Range(1, 1000)]
    public float maxHealth, weight;
    [SerializeField]
    float health, headSpeed;
    GameObject Head;
    public GameObject HeadInit;
    CircleCollider2D headCollider;

    void Start()
    {
        Physics2D.IgnoreLayerCollision(12,7);
        Physics2D.IgnoreLayerCollision(12, 10);
        health = maxHealth;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Strike();
        }
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
}
