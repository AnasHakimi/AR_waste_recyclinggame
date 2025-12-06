using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Level2Manager : MonoBehaviour, ILevelManager
{
    [Header("Game Objects")]
    public List<GameObject> wastePrefabs;
    public Transform spawnPoint;
    public GameObject instructionCanvas;
    public Transform wasteParent;

    [Header("UI Elements")]
    public TMP_Text timerText;
    public TMP_Text collectedText;
    public TMP_Text wasteNameText;
    public GameObject endCanvas;
    public GameObject[] stars;
    public GameObject settingsPanel;

    [Header("Health Settings")]
    public GameObject[] hearts;
    private int health;
    private int maxHealth = 3;

    [Header("Game Settings")]
    public float levelTime = 60f;
    public int totalToSort = 10;

    [Header("Animator")]
    public Animator feedbackAnimator;

    [Header("Hand Canvas")]
    public GameObject handInstructionCanvas;
    private bool instructionShown = false;
    private bool instructionDismissed = false;


    // Private variables
    private float timer;
    private float previousTimeScale;
    private int collected = 0;
    private int wasteCount = 0;
    private bool gameStarted = false;
    private bool levelEnded = false;
    private bool isPaused = false;
    private GameObject currentWaste;

    void Start()
    {
        InitializeGame();
        instructionCanvas.SetActive(false);
    }

    void InitializeGame()
    {
        health = maxHealth;
        UpdateUI();
    }

    public void StartLevel()
    {
        if (gameStarted) return;

        gameStarted = true;
        levelEnded = false;
        timer = levelTime;
        collected = 0;
        health = maxHealth;
        wasteCount = 0;

        // Show hand instruction
        if (handInstructionCanvas != null)
        {
            handInstructionCanvas.SetActive(true);
            instructionShown = true;
            instructionDismissed = false;
        }

        SpawnWaste();
        UpdateUI();

    }

    void Update()
    {
        if (!gameStarted || levelEnded || isPaused) return;

        UpdateTimer();
        CheckLevelCompletion();
    }

    void UpdateTimer()
    {
        timer -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(timer).ToString();
    }

    void CheckLevelCompletion()
    {
        if (timer <= 0 || health <= 0 || collected >= totalToSort)
        {
            EndLevel();
        }
    }

    void SpawnWaste()
    {
        if (wastePrefabs.Count == 0) return;

        int index = Random.Range(0, wastePrefabs.Count);
        currentWaste = Instantiate(wastePrefabs[index], spawnPoint.position, Quaternion.identity, wasteParent);

        if (currentWaste.TryGetComponent<WasteItem>(out var wasteItem))
        {

            wasteItem.Initialize(spawnPoint.position);
            wasteNameText.text = wasteItem.wasteName;

            wasteItem.canBeThrown = false;
            StartCoroutine(EnableInputAfterDelay(wasteItem, 0.5f));
        }

        wasteCount++;
    }

    IEnumerator EnableInputAfterDelay(WasteItem wasteItem, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (wasteItem != null) wasteItem.canBeThrown = true;
    }

    public void OnWasteSorted(bool correct)
    {
        if (levelEnded) return;

        if (correct)
        {
            collected++;
            AudioManager.instance?.PlaySFX("Correct");

            if (feedbackAnimator != null)
            {
                feedbackAnimator.SetBool("IsYeay", true);
                Invoke(nameof(ResetYeayBool), 0.5f);

            }
        }
        else
        {
            health = Mathf.Max(0, health - 1); // Ensure health doesn't go below 0
            AudioManager.instance?.PlaySFX("Wrong");

            if (feedbackAnimator != null)
            {
                feedbackAnimator.SetBool("IsSalah", true);
                Invoke(nameof(ResetSalahBool), 0.5f);

            }
        }

        DestroyCurrentWaste();

        if (collected >= totalToSort || health <= 0)
        {
            EndLevel();
        }
        else
        {
            SpawnWaste();
        }

        UpdateUI();
    }

    void ResetYeayBool()
    {
        feedbackAnimator?.SetBool("IsYeay", false);
    }

    void ResetSalahBool()
    {
        feedbackAnimator?.SetBool("IsSalah", false);
    }

    public void OnWasteGrabbed(WasteItem wasteItem)
    {
        if (wasteNameText != null)
        {
            wasteNameText.text = wasteItem.wasteName;
        }

        // Hide hand instruction on first correct tap
        if (instructionShown && !instructionDismissed)
        {
            if (handInstructionCanvas != null)
                handInstructionCanvas.SetActive(false);

            instructionDismissed = true;
        }
    }


    void DestroyCurrentWaste()
    {
        if (currentWaste != null)
        {
            Destroy(currentWaste);
            currentWaste = null; // Clear reference
        }
    }

    void UpdateUI()
    {
        collectedText.text = $"{collected} / {totalToSort}";

        // Update hearts display - similar to your star logic
        for (int i = 0; i < maxHealth; i++)
        {
            if (hearts.Length > i + maxHealth) // Safety check
            {
                hearts[i].SetActive(i < health);          // Red hearts
                hearts[i + maxHealth].SetActive(i >= health); // Gray hearts
            }
        }
    }

    void EndLevel()
    {
        levelEnded = true;
        gameStarted = false;
        DestroyCurrentWaste();
        ShowEndCanvas();
    }

    void ShowEndCanvas()
    {
        int earnedStars = CalculateEarnedStars();
        UpdateStarDisplay(earnedStars);

        AudioManager.instance?.musicSource.Stop();
        AudioManager.instance?.PlaySFX(health > 0 ? "Level Pass" : "Level Fail");
        endCanvas.SetActive(true);
    }

    int CalculateEarnedStars()
    {
        if (health <= 0 || timer <= 0f) return 0; // Fail condition
        return Mathf.Clamp(health, 1, 3); // 1, 2, or 3 stars based on remaining health
    }

    void UpdateStarDisplay(int earnedStars)
    {
        for (int i = 0; i < stars.Length / 2; i++)
        {
            stars[i].SetActive(i < earnedStars);
            stars[i + (stars.Length / 2)].SetActive(i >= earnedStars);
        }
    }

    #region Pause and Navigation
    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void OpenInstructionMenu()
    {
        if (levelEnded) return;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        instructionCanvas.SetActive(true);
        isPaused = true;
        AudioManager.instance?.musicSource.Pause();
    }

    public void CloseInstructionMenu()
    {
        Time.timeScale = previousTimeScale;
        instructionCanvas.SetActive(false);
        isPaused = false;
        AudioManager.instance?.musicSource.UnPause();
    }

    public void PauseGame()
    {
        if (levelEnded) return;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        settingsPanel.SetActive(true);
        isPaused = true;
        AudioManager.instance?.musicSource.Pause();
    }

    public void ResumeGame()
    {
        Time.timeScale = previousTimeScale;
        settingsPanel.SetActive(false);
        isPaused = false;
        AudioManager.instance?.musicSource.UnPause();
    }

    public void RestartLevel()
    {
        ResetTimeScale();
        AudioManager.instance?.musicSource.Play();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoHome()
    {
        ResetTimeScale();
        AudioManager.instance?.musicSource.Play();
        SceneManager.LoadScene("Main Menu");
    }

    public void NextLevel()
    {
        ResetTimeScale();
        AudioManager.instance?.musicSource.Play();
        SceneManager.LoadScene("AR");
    }

    void ResetTimeScale()
    {
        Time.timeScale = 1f;
    }
    #endregion
}