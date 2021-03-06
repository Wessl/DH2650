using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDropper : MonoBehaviour
{
    public GameObject enemyToKillForDrop;

    private SpriteRenderer sr;
    private BoxCollider2D[] cols;
    private Rigidbody2D rb;
    public CastleDoorOpener castleDoor;

    private Vector3 spawnPos;

    private bool keyIsPickedUp;

    private bool keyCanBePickedUp;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        keyCanBePickedUp = false;
        keyIsPickedUp = false;
        sr = GetComponent<SpriteRenderer>();
        sr.color = Color.clear;
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        cols = GetComponents<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyToKillForDrop == null)
        {
            sr.color = Color.white;
            rb.WakeUp();
            transform.position = spawnPos;
            rb.AddForce(new Vector2(0,1), ForceMode2D.Impulse);
            Invoke("KeyActivation", 0.5f);
        }
        else
        {
            spawnPos = enemyToKillForDrop.transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && keyCanBePickedUp)
        {
            keyIsPickedUp = true;
            castleDoor.GetKey();
            Debug.Log("Key picked up");
            audioSource.Play();
            sr.sprite = null;
            foreach (var col in cols)
            {
                col.enabled = false;
                col.isTrigger = true;
            }
            rb.Sleep();
            Destroy(gameObject, 0.5f);
        }
    }

    void KeyActivation()
    {
        keyCanBePickedUp = true;
    }
}
