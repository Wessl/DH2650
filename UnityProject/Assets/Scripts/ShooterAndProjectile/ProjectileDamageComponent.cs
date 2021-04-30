using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDamageComponent : MonoBehaviour
{
    // This gets a separate script because parent object is on the enemy layer, which is not supposed to interact with player layer. 
    // This one is on the default layer which can interact with the player and then deal damage. 

    private float damageToDeal;
    private bool canDamageShooter;

    private void Start()
    {
        damageToDeal = GetComponentInParent<Projectile>().DamageToDeal;
        canDamageShooter = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("touching player");
            other.GetComponent<Combat>().TakeDamage(damageToDeal);
        } else if (other.CompareTag("StationaryShootingEnemy") && canDamageShooter)
        {
            other.GetComponentInParent<ShooterBoye>().TakeDamage(damageToDeal);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("StationaryShootingEnemy"))
        {
            // We have left our shooter. Activate ability to take damage from our own projectile.
            canDamageShooter = true;
        }
    }
}
