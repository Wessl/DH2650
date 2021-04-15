using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    Vector2 worldPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate();
    }

    void Rotate()
    {
        transform.RotateAround(new Vector2(94, -1), new Vector3(0,0,1), 20 * Time.deltaTime);
    }
}
