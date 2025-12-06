using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static string sceneName;

    public void Animationbttn()
    {
        GetComponent<Animation>().Play("buttonAni");
        AudioManager.instance?.PlaySFX("Button");
    }

    public void next_scene(string name)
    {
        this.gameObject.SetActive(true);
        sceneName = name;
        GetComponent<Animator>().Play("end");
    }

    public void Object_InActive()
    {
        this.gameObject.SetActive(false);
    }


    public void Next_Scene()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    
}

