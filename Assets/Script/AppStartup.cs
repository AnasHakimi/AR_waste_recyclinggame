using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppStartup : MonoBehaviour
{
    void Start()
    {
        SceneLoader.targetScene = "Intro"; // next stop after loading (bug kalau dah skip intro nnti dia show intro 1 sec)
        SceneManager.LoadScene("Loading"); 
    }
}
