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
            other.GetComponent<Combat>().TakeDamage(damageToDeal);
        } else if (other.CompareTag("StationaryShootingEnemy") && canDamageShooter)
        {
            other.GetComponentInParent<ShooterBoye>().TakeDamage(damageToDeal);
        } else if (other.CompareTag("StationaryShootingBoss") && canDamageShooter && other.gameObject.TryGetComponent(out ShooterBoss boss))
        {
            boss.TakeDamage(damageToDeal);
        }
        else if (other.CompareTag("StationaryShootingBoss") && canDamageShooter && other.gameObject.TryGetComponent(out ShooterBossEnrageSpawn bossEnrageSpawn))
        {
            bossEnrageSpawn.TakeDamage(damageToDeal);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("StationaryShootingEnemy") || other.CompareTag("StationaryShootingBoss"))
        {
            // We have left our shooter. Activate ability to take damage from our own projectile.
            StartCoroutine(ActivateShooting());
        }
    }

    IEnumerator ActivateShooting()
    {
        yield return new WaitForSeconds(0.5f);
        canDamageShooter = true;
    }
}
