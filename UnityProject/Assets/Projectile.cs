using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damageToDeal;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("touching player");
            other.GetComponent<Combat>().TakeDamage(damageToDeal);
        }
    }

    public float DamageToDeal
    {
        get => damageToDeal;
        set => damageToDeal = value;
    }
}