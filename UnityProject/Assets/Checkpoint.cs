using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    public bool sceneTransition;
    public Vector3 nextScenePosition;
    public int nextSceneIndex;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if(!sceneTransition)    // If not a scene transition checkpoint, just set a new local checkpoint position
                SceneHandler.Instance.checkpointPosition = transform.position;
            else
            {   
                // If it's a scene transition, set the checkpoint position to where you want the player to spawn in the next scene and then load it
                SceneHandler.Instance.checkpointPosition = nextScenePosition;
                SceneHandler.Instance.LoadNewScene(nextSceneIndex);
            }
        }
    }
}
