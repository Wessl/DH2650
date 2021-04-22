using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Pathfinding;

public class Larva: MonoBehaviour
{
    public static Enemy instance;
    private Animator animator;
    private Material spriteMat;
    public Transform attackPoint, platform;
    [Tooltip("Time it takes to accelerate/decelerate between min and max movespeed")]
    public float attackDamage, weight, speed;
    public LayerMask playerLayer;
    Vector2 upperLeft, upperRight, bottomLeft, bottomRight;

    bool falling;
    float fallTime;

    void Start()
    {
        float x = platform.localScale.x;
        float y = platform.localScale.y;
        upperRight = (Vector2)platform.position + new Vector2(x / 2, y / 2);
        upperLeft = upperRight - new Vector2(x, 0);
        bottomLeft = upperLeft - new Vector2(0, y);
        bottomRight = bottomLeft + new Vector2(x, 0);
        animator = GetComponent<Animator>();
        spriteMat = GetComponentInChildren<SpriteRenderer>().material;
    }

    private void FixedUpdate()
    {
        Move();
        AttackHit();
    }

    // Move around target platform if placed on the platform's edge
    void Move()
    {
        Vector2 pos = transform.position;
        Vector2 newPos = pos;
        if (pos.x == upperLeft.x && !(pos.y == bottomLeft.y))
        {
            transform.eulerAngles = new UnityEngine.Vector3(0, 0, 90);
            newPos = Vector2.MoveTowards(pos, bottomLeft, speed);
        }
        else if (pos.y == bottomLeft.y && !(pos.x == bottomRight.x))
        {
            transform.eulerAngles = new UnityEngine.Vector3(0, 0, 180);
            newPos = Vector2.MoveTowards(pos, bottomRight, speed);
        }
        else if (pos.x == bottomRight.x && !(pos.y == upperRight.y))
        {
            transform.eulerAngles = new UnityEngine.Vector3(0, 0, 270);
            newPos = Vector2.MoveTowards(pos, upperRight, speed);
        }
        else if (pos.y == upperRight.y && !(pos.x == upperLeft.x))
        {
            transform.eulerAngles = new UnityEngine.Vector3(0, 0, 0);
            newPos = Vector2.MoveTowards(pos, upperLeft, speed);
        }
        transform.position = newPos;
    }
    // Called from the animation behaviour state exit function
    public void AttackHit()
    {
        Collider2D hitCollider = Physics2D.OverlapCapsule(attackPoint.position, new Vector2(1, 0.4f), CapsuleDirection2D.Horizontal, 0, playerLayer);
        if (hitCollider)
        {
            hitCollider.gameObject.GetComponent<Combat>().TakeDamage(attackDamage);
            //print(target.gameObject.layer);

            //Combat player = target.GetComponent<Combat>();
            //player.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage()
    {
        Die();
    }

    // Flash sprite white to indicate damage being taken
    IEnumerator FlashSprite()
    {
        spriteMat.SetColor("_Color", new Color(1, 0.55f, 0.55f, 0.88f));   // Arbitrary values
        yield return new WaitForSeconds(0.07f);                                          // Arbitrary wait time, roughly four frames @60 fps
        // Reset to base value
        spriteMat.SetColor("_Color", new Color(1, 1, 1, 0));               // Alpha 0 = default sprite color
    }

    void Die()
    {
        // Remove enemy gameObject from scene. 
        Destroy(gameObject);
    }

}
