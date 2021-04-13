using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableGameButton : MonoBehaviour
{
    public Sprite downPressedButton;
    public Sprite unPressedButton;
    public SpriteRenderer sprRen;
    private bool isPressed;
    public ButtonActionable linkedGameObject;
    private BoxCollider2D boxCol;

    private void Start()
    {
        sprRen = GetComponent<SpriteRenderer>();
        boxCol = GetComponent<BoxCollider2D>();
        isPressed = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // if the collider happens to be the tongue or just the frog...
        if (other.CompareTag("Tongue") || other.CompareTag("Player"))
        {
            Debug.Log("button clicked!");
            ToggleButton();
        }
    }

    void ToggleButton()
    {
        // Swap the sprite
        if (sprRen.sprite == downPressedButton)
        {
            sprRen.sprite = unPressedButton;
            boxCol.size = new Vector2(boxCol.size.x, boxCol.size.y * 1.5f);     // Adjust collider size with button
            isPressed = false;
        }
        else
        {
            sprRen.sprite = downPressedButton;
            boxCol.size = new Vector2(boxCol.size.x, boxCol.size.y / 1.5f);     // Adjust collider size
            isPressed = true;
        }
        linkedGameObject.ButtonAction(isPressed);
        
    }
}
