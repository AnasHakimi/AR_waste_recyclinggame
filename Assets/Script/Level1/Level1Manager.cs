using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Level1Manager : MonoBehaviour, ILevelManager
{
    public List<GameObject> wastePrefabs;
    public List<Transform> spawnLocations;
    public RobotCollector robot;

    private int totalWastes = 0;
    private int collected = 0;

    public TMP_Text timerText;
    public TMP_Text collectedText;
    public TMP_Text speedMultiplierText; // New UI element to show current speed

    public float levelTime = 60f;
    private float timer;
    private bool levelEnded = false;
    private bool gameStarted = false;

    // Time acceleration parameters
    public float initialTimeScale = 1f;
    public float maxTimeScale = 3f;
    public float timeScaleIncrement = 0.1f;
    private float currentTimeScale;

    // End screen UI
    public GameObject endCanvas;
    public GameObject[] stars; // 0-2 = yellow, 3-5 = black

    public Transform wasteParent; // Assign to levelRoot or a child like "WastesContainer"
    public float threeStarTime = 25f;
    public float twoStarTime = 45f;

    public GameObject settingsPanel;
    private bool isPaused = false;
    private float previousTimeScale = 1f;

    public Spin spinManager;

    public GameObject handInstructionCanvas;
    private bool instructionShown = false;
    private bool instructionDismissed = false;

    public GameObject instructionCanvas;


    void Start()
    {
        currentTimeScale = initialTimeScale;
        endCanvas.SetActive(false);
        instructionCanvas.SetActive(false);
        if (speedMultiplierText) speedMultiplierText.text = "Speed: 1.0x";
    }

    public void StartLevel()
    {
        if (gameStarted) return;

        gameStarted = true;
        timer = levelTime;
        currentTimeScale = initialTimeScale;
        SpawnAllWastes();
        endCanvas.SetActive(false);
        UpdateUI();

        // Show hand instruction
        if (handInstructionCanvas != null)
        {
            handInstructionCanvas.SetActive(true);
            instructionShown = true;
            instructionDismissed = false;
        }
    }

    void Update()
    {
        if (!gameStarted || levelEnded) return;


        // Time passes faster as currentTimeScale increases
        timer -= Time.deltaTime * currentTimeScale;
        UpdateTimerDisplay();

        if (timer <= 0)
        {
            timer = 0;
            levelEnded = true;
            ShowEndCanvas();
        }
    }

    void UpdateTimerDisplay()
    {
        timerText.text = timer <= 5f ? timer.ToString("0.0") : Mathf.CeilToInt(timer).ToString();
    }

    void SpawnAllWastes()
    {
        if (wastePrefabs.Count == 0 || spawnLocations.Count == 0)
        {
            Debug.LogError("Missing waste prefabs or spawn locations!");
            return;
        }

        foreach (Transform spawnPoint in spawnLocations)
        {
            int prefabIndex = Random.Range(0, wastePrefabs.Count);
            GameObject selectedPrefab = wastePrefabs[prefabIndex];

            //  Parent it under wasteParent
            GameObject spawned = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity, wasteParent);

            spinManager.RegisterObject(spawned);

        }

        totalWastes = spawnLocations.Count;
        UpdateUI();
    }

    public void OnWasteCollected()
    {
        if (levelEnded) return; // Do nothing if level already ended

        collected++;

        // Hide hand instruction on first correct tap
        if (instructionShown && !instructionDismissed)
        {
            if (handInstructionCanvas != null)
                handInstructionCanvas.SetActive(false);

            instructionDismissed = true;
        }

        // Increase time acceleration with each collection
        currentTimeScale = Mathf.Min(maxTimeScale, currentTimeScale + timeScaleIncrement);

        if (speedMultiplierText)
            speedMultiplierText.text = $"Speed: {currentTimeScale:0.0}x";

        AudioManager.instance?.PlaySFX("Collect");

        UpdateUI();

        if (collected >= totalWastes)
        {
            levelEnded = true;
            ShowEndCanvas();
        }
    }


    void UpdateUI()
    {
        collectedText.text = $"{collected} / {totalWastes}";
    }

    void ShowEndCanvas()
    {
        // Calculate actual time used (independent of time scaling)
        float realTimeUsed = levelTime - timer;
        int earnedStars = 0;

        if (robot != null)
        {
            robot.enabled = false;
        }

        if (collected == totalWastes)
        {
            if (realTimeUsed <= threeStarTime) earnedStars = 3;
            else if (realTimeUsed <= twoStarTime) earnedStars = 2;
            else earnedStars = 1;
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.musicSource.Stop();
            AudioManager.instance?.PlaySFX(earnedStars > 0 ? "Level Pass" : "Level Fail");
        }

        // Display stars
        for (int i = 0; i < 3; i++)
        {
            if (stars.Length > i + 3) // Safety check
            {
                stars[i].SetActive(i < earnedStars);
                stars[i + 3].SetActive(i >= earnedStars);
            }
        }

        if (endCanvas != null) endCanvas.SetActive(true);
    }

    // Called when settings button is clicked
    public void ToggleSettings()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void OpenInstructionMenu()
    {
        previousTimeScale = currentTimeScale;
        Time.timeScale = 0f;
        instructionCanvas.SetActive(true);
        isPaused = true;

        if (robot != null)
            robot.enabled = false;
        AudioManager.instance?.musicSource.Pause();
    }


    public void ResumeGame()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = previousTimeScale;
        isPaused = false;

        if (robot != null)
            robot.enabled = true;
        AudioManager.instance?.musicSource.UnPause();
    }

    public void CloseInstructionMenu()
    {
        instructionCanvas.SetActive(false);
        Time.timeScale = previousTimeScale;
        isPaused = false;

        if (robot != null)
            robot.enabled = true;
        AudioManager.instance?.musicSource.UnPause();
    }

    public void PauseGame()
    {
        previousTimeScale = currentTimeScale;
        Time.timeScale = 0f;
        settingsPanel.SetActive(true);
        isPaused = true;

        if (robot != null)
            robot.enabled = false;
        AudioManager.instance?.musicSource.Pause();
    }

    public void GoHome()
    {
        Time.timeScale = 1f; // Reset before loading
        AudioManager.instance?.musicSource.Play();
        SceneManager.LoadScene("Main Menu");
    }

    public void Nextlevel()
    {
        Time.timeScale = 1f; // Reset before loading
        AudioManager.instance?.musicSource.Play();
        SceneManager.LoadScene("AR");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // Reset before loading
        AudioManager.instance?.musicSource.Play();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}