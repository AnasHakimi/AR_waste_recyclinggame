using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VisibilityController : MonoBehaviour
{
    // Assign multiple objects in the Inspector
    public GameObject[] objectsToControl;

    // Call this when target is found
    public void HideObjects()
    {
        foreach (GameObject obj in objectsToControl)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    // Call this when target is lost
    public void ShowObjects()
    {
        foreach (GameObject obj in objectsToControl)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("Play"); 
    }

   
}
