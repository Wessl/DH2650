using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SnakeHead : MonoBehaviour
{

    SnakeBoss snakeScript;
    Rigidbody2D rb;
    public float maxLength, minLength;
    public GameObject headCenterInit;
    private GameObject headCenter;
    GameObject col;
    float dist;
    Vector2 relativePos;
    bool hit;
    // Start is called before the first frame update
    void Start()
    {
        GameObject snake = GameObject.FindGameObjectWithTag("SnakeBoss");
        snakeScript = snake.GetComponent<SnakeBoss>();
        rb = GetComponent<Rigidbody2D>();
        headCenter = Instantiate(headCenterInit, transform.position, quaternion.identity);
        Physics2D.IgnoreLayerCollision(12, 7);
        Physics2D.IgnoreLayerCollision(12, 10);
        Physics2D.IgnoreLayerCollision(12, 13);
    }

    // Update is called once per frame
    void Update()
    {
        dist = Vector2.Distance(snakeScript.headPoint.position, rb.position);

        // Draw line between tongue endpoint and player - maybe dumb?
        //Stretch(headCenter, snakeScript.headPoint.position, rb.position);

        /*
        if (dist > maxLength)   // the tongue isn't that long, time to pull back
        {
            snakeScript.RetractTongue();
        }*/

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hit)    // we only want the tongue to stick to one object, otherwise things get fucky..
            return;

        col = collision.gameObject;
        hit = true;
        rb.velocity = new Vector2(0, 0);
        print(col.tag);
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
        Vector3 scale = new Vector3(0.3f, 0.3f, 0.3f);               // Tongue center is too thick, scale down
        scale.y = Vector3.Distance(initialPosition, finalPosition) / spriteSize;
        tongue.transform.localScale = scale;
    }

    // When this is destroyed, remove the center of the tongue as well
    void OnDestroy()
    {
        Destroy(headCenter);
    }


}
