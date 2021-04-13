using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEdgeCheck : MonoBehaviour
{
    public Enemy enemyParent;
    private float turnAroundCooldownTime = 0.5f;
    private bool isTurningAround;

    void Start()
    {
        isTurningAround = false;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground") && !isTurningAround)
        {
            // This collider is no longer detecting any ground - thus I am on the edge of a cliff, and should turn around.
            enemyParent.FlipRotation();
            isTurningAround = true;
            StartCoroutine(waitForTurningAround());
        }
    }

    IEnumerator waitForTurningAround()
    {
        yield return new WaitForSeconds(turnAroundCooldownTime);
        isTurningAround = false;
    }

}
