using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWallCheck : MonoBehaviour
{
    public Enemy enemyParent;
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
            // This collider is in front of the enemy - if it detects "ground" it means a wall is in front - turn around
            enemyParent.FlipRotation();
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
