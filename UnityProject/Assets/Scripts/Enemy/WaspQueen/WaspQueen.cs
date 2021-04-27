using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaspQueen : MonoBehaviour
{
    public Transform player, returnPoint, leftBounds, rightBounds;
    public float speed;
    bool attacking;
    public Transform floor;
    Vector2 target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            Vector3 direction = (transform.position - player.position).normalized;
            float y = player.position.y - floor.position.y;
            float scale = y / direction.y;
            
            target = player.position - direction*scale;
            if (target.x > leftBounds.position.x && target.x < rightBounds.position.x)
                attacking = true;
        }
        if (Input.GetKey(KeyCode.Q))
            MoveBack();
        if (attacking)
            Attack();
    }


    void Attack()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target, step);
        if ((Vector2)transform.position == target)
            attacking = false;
    }

    void MoveBack()
    {
        float step = speed/2 * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(target.x, returnPoint.position.y), step);
    }
}