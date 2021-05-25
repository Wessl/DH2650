using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance;
    public string firstArea, secondArea, thirdArea, fourthArea, fifthArea;
    public string currentScene;
    public Vector3 checkpointPosition;
    private GameObject shrine;
    [Tooltip("Put the start positions for each level in order here")]
    public Vector3[] startPositions;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            // Destroy if it's a duplicate scene handler
            Destroy(gameObject);
            return;
        }
        // Make the scene handler persistent
        DontDestroyOnLoad(gameObject);

        currentScene = SceneManager.GetActiveScene().name;
        // Initialize first spawn point
        checkpointPosition = startPositions[SceneManager.GetActiveScene().buildIndex];
    }
    // Start is called before the first frame update
    void Start()
    {
        if(currentScene.Equals(firstArea))
            AudioManager.Instance.Play("Area 1 Theme");
        else if (currentScene.Equals(secondArea))
        {
            AudioManager.Instance.Play("Cave");
        } else if (currentScene.Equals(thirdArea))
        {
            AudioManager.Instance.Play("Castle");

        }
    }

    public void LoadNewScene(int i)
    {
        SceneManager.LoadScene(i);
        currentScene = SceneManager.GetSceneByBuildIndex(i).name;
        Debug.Log("HEY CURRENT SCENE IS NOW " + currentScene);
        if(currentScene.Equals(firstArea))
            AudioManager.Instance.Play("Area 1 Theme");
        else if (currentScene.Equals(secondArea))
        {
            AudioManager.Instance.Play("Cave");
            AudioManager.Instance.Stop("Area 1 Theme");
        } else if (currentScene.Equals(fourthArea))
        {
            AudioManager.Instance.Play("Castle");
            AudioManager.Instance.Stop("Cave");
        }
    }

    public void ExtinguishPreviousShrine(GameObject newShrine)
    {
        if (shrine)
        {
            shrine.GetComponent<Animator>().SetTrigger("Extinguish");
            shrine.GetComponent<Checkpoint>().lit = false;
        }
        shrine = newShrine;
    }
}
