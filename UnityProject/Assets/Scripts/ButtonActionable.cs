using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Is intended to be used in conjunction with a ClickableGameButton
public class ButtonActionable : MonoBehaviour
{
    public Vector3 moveDirectionAndAmount;
    public float doorMoveDuration = 1;
    private Vector3 destination;
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
        destination = originalPosition + moveDirectionAndAmount;
    }
    
    // Doors are simple for now, will simply move back and forth in some direction each time ButtonAction is called
    public void ButtonAction(bool isPressed)
    {
        if (isPressed)
        {

            StartCoroutine(LerpPosition(destination, doorMoveDuration));
        }
        else
        {
            StartCoroutine(LerpPosition(originalPosition, doorMoveDuration));
        }
        
    }

    // Move object to the position specified over some time
    IEnumerator LerpPosition(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }
}
