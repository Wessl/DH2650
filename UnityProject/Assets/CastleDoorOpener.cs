using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleDoorOpener : MonoBehaviour
{
    public Sprite openedDoor;
    private bool playerHasKey;

    private SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        playerHasKey = false;
    }

    public void GetKey()
    {
        playerHasKey = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playerHasKey)
        {
            sr.sprite = openedDoor;
        }
    }
}
