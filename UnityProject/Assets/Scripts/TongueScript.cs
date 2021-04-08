using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TongueScript : MonoBehaviour
{

    PlayerMovement playerMV;
    Rigidbody2D rb;
    public float maxLength, minLength;
    GameObject col;
    float dist;
    Vector2 relativePos;
    bool hit;
    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerMV = player.GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        dist = Vector2.Distance(playerMV.getMouthPos(), rb.position);
        
        if (dist > maxLength)   // the tongue isn't that long, time to pull back
        {
            playerMV.RetractTongue();
        }
    }

    private void FixedUpdate()
    {
        if (hit)
            rb.position = new Vector2(col.transform.position.x, col.transform.position.y) + relativePos;    // updates tonguetip to stick on the position it hit
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        dist = Vector2.Distance(playerMV.getMouthPos(), rb.position);
        if (dist < minLength)
        {
            print(minLength + "dist " + dist);
            playerMV.RetractTongue();
        }

        col = collision.gameObject;
        hit = true;
        rb.velocity = new Vector2(0, 0);
        print(col.tag);
        playerMV.TargetHit(col, rb.position, col.tag);
        relativePos = rb.position - new Vector2(col.transform.position.x, col.transform.position.y);
    }


}
