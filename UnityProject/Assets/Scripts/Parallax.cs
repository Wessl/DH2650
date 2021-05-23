using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length, startPos;
    public GameObject camera;
    public float parallaxStrength;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        float tmp = camera.transform.position.x * (1 - parallaxStrength);
        float dist = camera.transform.position.x * parallaxStrength;

        transform.position = new Vector3(startPos + dist, transform.position.y, transform.position.z);
        if (tmp > startPos + length)
            startPos += length;
        else if (tmp < startPos - length)
            startPos -= length;
    }
}
