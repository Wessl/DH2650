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
    private AudioSource audioSource;

    private bool isOpened;
    // Start is called before the first frame update
    void Start()
    {
        isOpened = false;
        sr = GetComponent<SpriteRenderer>();
        playerHasKey = false;
        col = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    public void GetKey()
    {
        playerHasKey = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playerHasKey && !coloredDoor && !isOpened)
        {
            sr.sprite = openedDoor;
            col.enabled = false;
            audioSource.Play();
            isOpened = true;
        } else if (other.CompareTag("Player") && playerHasKey && coloredDoor && !isOpened)
        {
            transform.localRotation = new Quaternion(0, 0, 0, 0);
            col.enabled = false;
            audioSource.Play();
            isOpened = true;
        }
        
    }
}
