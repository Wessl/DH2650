using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterBossButtonActivator : MonoBehaviour
{
    private bool buttonIsActive;
    public Transform ogPos;
    public Transform endPos;
    private int numEnemies;
    private bool hasMoved;

    void Start()
    {
        hasMoved = false;
        ogPos.parent = null;
        endPos.parent = null;
    }

    private void Update()
    {
        var enemies = GameObject.FindGameObjectsWithTag("StationaryShootingBoss");
        Debug.Log("enemy amount " + enemies.Length);
        numEnemies = 0;
        foreach (var enemy in enemies)
        {
            if (enemy.TryGetComponent(out ShooterBoss boss))
            {
                if (boss.Alive)
                {
                    numEnemies++;
                }
            } else if (enemy.TryGetComponent(out ShooterBossEnrageSpawn bossEnrageSpawn))
            {
                if (bossEnrageSpawn.Alive)
                {
                    numEnemies++;
                }
            }
        }

        if (numEnemies == 0 && !hasMoved)
        {
            StartCoroutine(LerpPosition());
        }

    }

    IEnumerator LerpPosition()
    {
        hasMoved = true;
        float time = 0;
        var duration = 2f;
        while (time < duration && numEnemies == 0)
        {
            transform.position = Vector3.Lerp(ogPos.position, endPos.position, time/duration);
            time += Time.deltaTime;
            yield return null;
        }
    }
    
}
