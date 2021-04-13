using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeWallCheck : MonoBehaviour
{
    public SnakeController snakeParent;
    private float turnAroundCooldownTime = 0.5f;
    private bool isTurningAround;

    void Start()
    {
        isTurningAround = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ground") && !isTurningAround)
        {
            // This collider is in front of the snake - if it detects "ground" it means a wall is in front - turn around
            snakeParent.FlipRotation();
            isTurningAround = true;
            StartCoroutine(waitForTurningAround());
        }
    }

    // If the turn can happen on every frame, snake sometimes gets stuck turning around frantically
    IEnumerator waitForTurningAround()
    {
        yield return new WaitForSeconds(turnAroundCooldownTime);
        isTurningAround = false;
    }
}
