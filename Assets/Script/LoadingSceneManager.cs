using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    public Slider progressBar;
    public float minimumLoadingTime = 4f;     //second
    public float fillSpeed = 0.5f;          // speed of the slider fill

    private float targetProgress = 0f;

    void Start()
    {
        StartCoroutine(LoadTargetScene());
    }

    IEnumerator LoadTargetScene()
    {
        if (string.IsNullOrEmpty(SceneLoader.targetScene))
        {
            Debug.LogError("SceneLoader.targetScene is empty!");
            yield break;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneLoader.targetScene);
        operation.allowSceneActivation = false;

        float elapsed = 0f;

        while (!operation.isDone)
        {
            elapsed += Time.deltaTime;

            // Update target progress (0.0 to 0.9)
            targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Smoothly update the slider value
            if (progressBar != null)
            {
                progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, fillSpeed * Time.deltaTime);
            }

            // Check if we can activate the scene
            if (operation.progress >= 0.9f && elapsed >= minimumLoadingTime && progressBar.value >= 0.99f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
