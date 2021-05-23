using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    public bool sceneTransition;
    public Vector3 nextScenePosition;
    public int nextSceneIndex;

    bool playerInCollider;
    public bool lit;

    public GameObject textCanvas;
    private Animator animator;
    

    private void Awake()
    {
        if (sceneTransition)
            return;
        textCanvas.active = false;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(playerInCollider)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                SceneHandler.Instance.checkpointPosition = transform.position;
                SceneHandler.Instance.ExtinguishPreviousShrine(gameObject);
                animator.SetTrigger("Ignite");
                lit = true;
                playerInCollider = false;
                textCanvas.active = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if (!sceneTransition)    // If not a scene transition checkpoint, just set a new local checkpoint position
            {
                if (lit)
                    return;
                playerInCollider = true;
                textCanvas.active = true;
            }
            else
            {
                // If it's a scene transition, set the checkpoint position to where you want the player to spawn in the next scene and then load it
                SceneHandler.Instance.ExtinguishPreviousShrine(null);
                SceneHandler.Instance.checkpointPosition = nextScenePosition;
                SceneHandler.Instance.LoadNewScene(nextSceneIndex);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (playerInCollider)
        {
            playerInCollider = false;
            textCanvas.active = false;
        }
    }
}
