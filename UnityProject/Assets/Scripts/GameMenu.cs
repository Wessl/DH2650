using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public GameObject areYouSureYouWantToQuitPanel;
    public GameObject optionsPanel;
    public Text actualLeftMouseAction;
    public Text actualRightMouseAction;
    public Slider sfxVolumeSlider;
    void Start()
    {
        // Make sure some playerprefs exist
        if (!PlayerPrefs.HasKey("LeftMouseIsTongue"))
        {
            PlayerPrefs.SetInt("LeftMouseIsTongue", 1);
        }

        if (PlayerPrefs.GetInt("LeftMouseIsTongue") == 1)
        {
            actualLeftMouseAction.text = "Tongue";
            actualRightMouseAction.text = "Attack";
        }
        else
        {
            actualLeftMouseAction.text = "Attack";
            actualRightMouseAction.text = "Tongue";
        }

        sfxVolumeSlider.value = AudioListener.volume;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            // Cancel == escape by default. Open up options panel if not open, if it is, close it.
            if (optionsPanel.activeSelf)
            {
                optionsPanel.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                optionsPanel.SetActive(true);
                Time.timeScale = 0;
            }
            
        } 
    }

    // Reload active scene - no save state behaviour just yet.
    public void DeathRespawnButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        areYouSureYouWantToQuitPanel.SetActive(true);
    }

    public void ExitGameYes()
    {
        Application.Quit();
    }

    public void ExitGameNo()
    {
        areYouSureYouWantToQuitPanel.SetActive(false);
    }

    public void CloseThisMenu()
    {
        optionsPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        Debug.Log("Not yet implemented! Coming soon");
    }
    
    // Ability to change button layout
    public void SwapMouseButtonMapping()
    {
        if (PlayerPrefs.GetInt("LeftMouseIsTongue") == 1)
        {
            // If == 1, then true. Now swap. 
            PlayerPrefs.SetInt("LeftMouseIsTongue", 0);
            actualLeftMouseAction.text = "Attack";
            actualRightMouseAction.text = "Tongue";
        }
        else
        {
            PlayerPrefs.SetInt("LeftMouseIsTongue", 1);
            actualLeftMouseAction.text = "Tongue";
            actualRightMouseAction.text = "Attack";
        }
        // Actually update the player scripts so the correct buttons are used
        Combat.instance.UpdateAttackButtonMapping();
        PlayerMovement.instance.UpdateTongueButtonMapping();
    }

    public void OnSFXVolSliderChange()
    {
        AudioListener.volume = sfxVolumeSlider.value;
    }
}
