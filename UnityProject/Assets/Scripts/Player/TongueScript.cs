using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TongueScript : MonoBehaviour
{

    PlayerMovement playerMV;
    Rigidbody2D rb;
    public float maxLength, minLength;
    public GameObject tongueCenterInit;
    private GameObject tongueCenter;
    GameObject target;
    float dist;
    Vector2 relativePos;
    bool hit;

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerMV = player.GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        tongueCenter = Instantiate(tongueCenterInit, transform.position, quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        dist = Vector2.Distance(playerMV.getMouthPos(), rb.position);
        
        // Draw line between tongue endpoint and player - maybe dumb?
        Stretch(tongueCenter, playerMV.getMouthPos(), rb.position);
        
        if (dist > maxLength)   // the tongue isn't that long, time to pull back
        {
            playerMV.RetractTongue();
        }
        
    }

    private void FixedUpdate()
    {
        if (hit)
            rb.position = new Vector2(target.transform.position.x, target.transform.position.y) + relativePos;    // updates tonguetip to stick on the position it hit
    }

    public void Target(GameObject hitTarget, Vector2 hitPos)
    {
        hit = true;
        target = hitTarget;
        rb.velocity = new Vector2(0, 0);
        relativePos = hitPos - new Vector2(target.transform.position.x, target.transform.position.y);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Nongrappable"))
        {
            playerMV.RetractTongue();
        }
    }
    
    // use this to stretch the gameobject between two points, center of player object and tongue end position in this case.
    // there is probably a much easier way of doing this...
    public void Stretch(GameObject tongue, Vector3 initialPosition, Vector3 finalPosition)
    {
        var sprite = tongue.GetComponent<SpriteRenderer>().sprite;
        float spriteSize = sprite.rect.height / sprite.pixelsPerUnit;
        Vector3 centerPos = (initialPosition + finalPosition) / 2f;
        tongue.transform.position = centerPos;
        Vector3 direction = finalPosition - initialPosition;
        direction = Vector3.Normalize(direction);
        tongue.transform.up = direction;
        Vector3 scale = new Vector3(0.3f,0.3f,0.3f);               // Tongue center is too thick, scale down
        scale.y = Vector3.Distance(initialPosition, finalPosition) / spriteSize;
        tongue.transform.localScale = scale;
    }
    
    // When this is destroyed, remove the center of the tongue as well
    void OnDestroy()
    {
        Destroy(tongueCenter);
    }


}
