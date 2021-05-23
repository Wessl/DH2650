using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleDoorOpener : MonoBehaviour
{
    public Sprite openedDoor;
    private bool playerHasKey;
    private BoxCollider2D col;

    private SpriteRenderer sr;
    public bool coloredDoor;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        playerHasKey = false;
        col = GetComponent<BoxCollider2D>();
    }

    public void GetKey()
    {
        playerHasKey = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playerHasKey && !coloredDoor)
        {
            sr.sprite = openedDoor;
            col.enabled = false;
        } else if (other.CompareTag("Player") && playerHasKey && coloredDoor)
        {
            transform.localRotation = new Quaternion(0, 0, 0, 0);
            col.enabled = false;
        }
    }
}
