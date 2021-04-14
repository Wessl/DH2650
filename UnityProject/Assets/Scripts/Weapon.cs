using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

// Class to be used for Weapons. Weapon type, how it's holstered, actions when attacking etc. should be done from this. 
public class Weapon : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement pm;
    private PolygonCollider2D col;
    private SpriteRenderer weaponSprite;
    private bool hasHitEnemy;
    [Range(1,100)]
    public float damageAmount;
    

    void Start()
    {
        anim = GetComponent<Animator>();
        pm = GetComponentInParent<PlayerMovement>();
        col = GetComponent<PolygonCollider2D>();
        weaponSprite = GetComponent<SpriteRenderer>();

        col.enabled = false;    // Weapon collider should be disabled while holstered
        hasHitEnemy = false;
    }
    
    void Update()
    {
        HolsterOrientation();
        Attack();
    }

    // This is kind of dirty code, but it works. Makes sure the sword is resting in it's holster on the correct side of the frog depending on which way frog is facing. 
    void HolsterOrientation()
    {
        if (pm.Orientation > 0)
        {
            weaponSprite.sortingOrder = -1;
        }
        else
        {
            weaponSprite.sortingOrder = 0;
        }
    }

    void Attack()
    {
        // Right click to attack
        if (Input.GetMouseButtonDown(1) && !hasHitEnemy)
        {
            // Activate weapon collider - 
            col.enabled = true;
            // Play animation
            anim.SetTrigger("attack");
        }
    }

    // Turn off collision just after hitting something - otherwise sword bugs out when hitting something, also too many particles are spawned
    // Long term bad implication? Only allows hitting on enemy per strike of the sword?
    void OnCollisionEnter2D(Collision2D col2d)
    {
        Debug.Log(col2d.collider.tag);
        if (col2d.collider.CompareTag("Enemy"))
        {
            col.enabled = false;
            hasHitEnemy = true;
        }
    }

    public void DisableCollision()
    {
        col.enabled = false;
        hasHitEnemy = false;
    }
    public float Damage
    {
        get => damageAmount;
        set => damageAmount = value;
    }
}
