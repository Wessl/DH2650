using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Win : MonoBehaviour
{
    public GameObject bossEnemy;

    private void Update()
    {
        if (bossEnemy == null)
        {
            GetComponent<Text>().text = "The toad king has been defeated! Well done. \n [Demo Over]" ;
        }
    }
}
