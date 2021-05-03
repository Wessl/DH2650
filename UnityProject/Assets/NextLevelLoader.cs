using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelLoader : MonoBehaviour
{
    [Tooltip("Should the next scene in the build index settings be loaded? If false, select which index should be used")]
    public bool autoLoadNextScene = true;

    public int sceneToLoad;

    private void OnTriggerEnter2D(Collider2D other)
    {   
        // If player collides 
        if (other.CompareTag("Player"))
        {
            if (autoLoadNextScene)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
        
        
    }
}
