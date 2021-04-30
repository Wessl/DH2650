using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damageToDeal;
    private Rigidbody2D rb;
    public ParticleSystem destroyPS;
    private CircleCollider2D collider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        transform.rotation *= Quaternion.Euler(0, 0, 720 * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Combat>().TakeDamage(damageToDeal);
            Die();
        }
        else if(other.CompareTag("Platform"))
        {
            // Don't want projectile to travel through stationary platform (after a short time)
            Die();
        }
    }

    public float DamageToDeal
    {
        get => damageToDeal;
        set => damageToDeal = value;
    }

    public void Deflect()
    {
        // reverse direction and add some speed
        rb.velocity *= -1.5f;
    }

    private void Die()
    {
        Instantiate(destroyPS, transform.position, Quaternion.identity);
        // just remove the sprite and delay the destruction. Otherwise, if you grab it with tongue the same frame it touches you, tongue can get null ref exception
        gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        collider.enabled = false;
        Destroy(gameObject, 0.5f);
    }
}
