using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to be used for Weapons. Weapon type, how it's holstered, actions when attacking etc. should be done from this. 
public class Weapon : MonoBehaviour
{
    private PlayerMovement pm;
    private SpriteRenderer weaponSprite;

    void Start()
    {
        pm = GetComponentInParent<PlayerMovement>();
        weaponSprite = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        HolsterOrientation();
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
}
