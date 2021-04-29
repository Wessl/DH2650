using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ShooterBoye : MonoBehaviour
{
    public GameObject body;
    public LayerMask playerLayer;
    public float radius;
    public float detectionDelay;
    private float timePassedSinceLastFindAttempt;
    private Transform playerTransform;
    private bool playerIsInRange;
    public GameObject projectile;
    public float projectileSpeed;
    public float damagePerShot;
    public float shootDelay;
    private float timeSinceLastShot;
    private Rigidbody2D rb;
    public float health;
    public ParticleSystem deathParticleSystem;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        timePassedSinceLastFindAttempt = 0;
        timeSinceLastShot = 1;
        playerIsInRange = false;
    }

    // Update is called once per frame
    void Update()
    {
        timePassedSinceLastFindAttempt += Time.deltaTime;
        if (timePassedSinceLastFindAttempt > detectionDelay)
        {
            FindPlayer();
            timePassedSinceLastFindAttempt = 0;
        }

        if (playerIsInRange)
        {
            FaceAndAttack();
        }
    }

    private void FindPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, playerLayer);
        foreach (var hit in hits)
        {
            if (hit.transform.CompareTag("Player"))
            {
                playerTransform = hit.transform;
                playerIsInRange = true;
            }
        }

        if (hits.Length == 0)
        {
            playerIsInRange = false;
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
        Instantiate(deathParticleSystem, body.transform.position, Quaternion.identity);
        Destroy(body);      // Don't need to destroy the foundation of the shooter boye, only body
    }

    void FaceAndAttack()
    {
        // Face player
        Vector3 targ = playerTransform.position;
        targ.z = 0f;
        Vector3 objectPos = body.transform.position;
        targ.x = targ.x - objectPos.x;
        targ.y = targ.y - objectPos.y;
 
        float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
        body.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        
        // Attack player
        if (timeSinceLastShot > shootDelay)
        {
            var proj = Instantiate(projectile, body.transform.position, quaternion.identity);
            proj.GetComponent<Rigidbody2D>().velocity = targ.normalized * projectileSpeed;
            proj.GetComponent<Projectile>().DamageToDeal = damagePerShot;
            StartCoroutine(DestroyProjectile(proj));
            timeSinceLastShot = 0;
        }
        else
        {
            timeSinceLastShot += Time.deltaTime;
        }
    }

    // Activate this when stationary enemy is no longer stuck to its foundation i.e. gets pulled by the player
    public void Unstuck()
    {
        rb.gravityScale = 1;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    IEnumerator DestroyProjectile(GameObject proj)
    {
        // destroy it after some time
        yield return new WaitForSeconds(10);
        Destroy(proj);
    }
}
