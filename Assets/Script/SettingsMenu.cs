using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsCanvas; 

    private void Start()
    {
        settingsCanvas.SetActive(false); 
    }

    public void OpenSettings()
    {
        settingsCanvas.SetActive(true);
    }

   
    public void CloseSettings()
    {
        settingsCanvas.SetActive(false);
    }

   
    public void GoHome()
    {
        
        SceneManager.LoadScene("Main Menu");
    }


    public void RestartLevel()
    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


}
