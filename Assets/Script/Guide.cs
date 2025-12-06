using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Guide : MonoBehaviour
{
    public string downloadLink = "https://drive.google.com/file/d/1knoJNr2ZeSz03DU5xtRkp0LFUyFCID_T/view?usp=drive_link";

    public void GoBack()
    {
        SceneManager.LoadScene("Main Menu"); 
    }

    public void Download()
    {
        Application.OpenURL(downloadLink);
    }
}
