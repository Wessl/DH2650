using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance;
    public string firstArea, secondArea, thirdArea;
    public string currentScene;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        currentScene = SceneManager.GetActiveScene().name;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(currentScene.Equals("Area1"))
            AudioManager.Instance.Play("Area 1 Theme");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
