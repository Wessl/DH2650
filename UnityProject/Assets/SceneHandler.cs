using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance;
    public string firstArea, secondArea, thirdArea;
    public string currentScene;
    public Vector3 checkpointPosition;

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
        checkpointPosition = new Vector3(-20, 6, 0);
    }
    // Start is called before the first frame update
    void Start()
    {
        /*
        if(currentScene.Equals("Area1"))
            AudioManager.Instance.Play("Area 1 Theme");*/
    }

    public void LoadNewScene(int i)
    {
        SceneManager.LoadScene(i);
    }
}
