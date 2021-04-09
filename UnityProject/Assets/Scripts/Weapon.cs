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
    

    void Start()
    {
        anim = GetComponent<Animator>();
        pm = GetComponentInParent<PlayerMovement>();
        col = GetComponent<PolygonCollider2D>();
        weaponSprite = GetComponent<SpriteRenderer>();

        col.enabled = false;    // Weapon collider should be disabled while holstered
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
        if (Input.GetMouseButtonDown(1))
        {
            // Remove from holster
            // Activate weapon collider
            col.enabled = true;
            // Play animation
            anim.SetTrigger("attack");
            // Put back into holster
        }
    }
}
