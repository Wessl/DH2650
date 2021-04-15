using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonFunctions : MonoBehaviour
{
    // Reload active scene - no save state behaviour just yet.
    public void DeathRespawnButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
