using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Level4Manager : MonoBehaviour, ILevelManager
{
    public static Level4Manager Instance;

    [System.Serializable]
    public class ItemUIInfo
    {
        public Sprite displayImage;
        public string itemName;
    }

    [Header("Item UI Info Lists")]
    public List<ItemUIInfo> containerPrefabDetails;
    public List<ItemUIInfo> transformedItemPrefabDetails;

    [Header("Item Detail Panel")]
    public GameObject itemInfoPanel;

    public UnityEngine.UI.Image oldItemImage;
    public TMP_Text oldItemText;

    public UnityEngine.UI.Image newItemImage;
    public TMP_Text newItemText;

    [Header("Game Objects")]
    public GameObject robot;
    public Transform machineTransform;
    public List<GameObject> containerPrefabs;
    public List<GameObject> transformedItemPrefabs;
    public GameObject redButton;
    public GameObject settingsPanel;
    public GameObject instructionCanvas;
    public Transform containerParent;
    public Transform itemHoldPoint;

    [Header("Transforms")]
    public Transform[] containerPositions;
    public Transform machineInputPoint;
    public Transform machineExitPoint;

    [Header("UI Elements")]
    public TMP_Text timerText;
    public TMP_Text completeText;
    public GameObject endCanvas;
    public GameObject[] stars;

    [Header("Game Settings")]
    public float levelTime = 90f;
    private float timer;
    private int completed = 0;
    private int totalToRecycle = 5;
    private bool gameStarted = false;
    private bool levelEnded = false;
    private bool isPaused = false;
    private float previousTimeScale;

    [Header("Hand Canvas")]
    public GameObject handInstructionCanvas;
    private bool instructionShown = false;
    private bool instructionDismissed = false;

    [Header("Robot Animation Settings")]
    public Animator robotAnimator;
    public float turnDuration = 0.5f;
    public float pickupAnimationTime = 1.7f;
    public float placeAnimationTime = 1.7f;

    private GameObject currentItem;
    private GameObject robotHeldItem;
    private GameObject currentTransformedItem;
    private int currentContainerIndex = -1;
    private int spawnIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        InitializeLevel();
        instructionCanvas.SetActive(false);
    }

    void InitializeLevel()
    {
        timer = levelTime;
        completed = 0;
        levelEnded = false;
        UpdateUI();
    }

    void Update()
    {
        if (!gameStarted || levelEnded || isPaused) return;

        timer -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(timer).ToString();

        if (timer <= 0 || completed >= totalToRecycle)
        {
            EndLevel();
        }

        // Mobile touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == redButton)
                {
                    // Play button sound effect
                    AudioManager.instance?.PlaySFX("Button");
                    OnRedButtonPressed();
                }
            }
        }

#if UNITY_EDITOR
        // Mouse input for Unity Editor testing
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == redButton)
                {
                    // Play button sound effect
                    AudioManager.instance?.PlaySFX("Button");
                    OnRedButtonPressed();
                }
            }
        }
#endif

    }

    public void StartLevel()
    {
        if (gameStarted) return;

        gameStarted = true;
        completed = 0;
        levelEnded = false;

        if (handInstructionCanvas != null)
        {
            handInstructionCanvas.SetActive(true);
            instructionShown = true;
            instructionDismissed = false;
        }

        SpawnNewContainer();
    }

    void SpawnNewContainer()
    {
        if (containerPrefabs.Count == 0 || containerPositions.Length == 0)
        {
            Debug.LogError("Missing container prefabs or positions!");
            return;
        }

        // Find an empty position
        List<Transform> emptyPositions = new List<Transform>();
        foreach (Transform pos in containerPositions)
        {
            if (Physics.OverlapSphere(pos.position, 0.5f).Length == 0)
            {
                emptyPositions.Add(pos);
            }
        }

        if (emptyPositions.Count == 0)
        {
            Debug.LogWarning("No empty container positions available!");
            return;
        }

        // Wrap around using modulo to loop through containerPrefabs
        currentContainerIndex = spawnIndex % containerPrefabs.Count;
        spawnIndex++;

        int posIndex = Random.Range(0, emptyPositions.Count);

        currentItem = Instantiate(
            containerPrefabs[currentContainerIndex],
            emptyPositions[posIndex].position,
            Quaternion.identity,
            containerParent
        );

        // Add visual feedback
        if (currentItem.TryGetComponent<Renderer>(out var renderer))
        {
            StartCoroutine(FlashObject(renderer, Color.green, 0.5f, 2));
        }
    }


    IEnumerator FlashObject(Renderer renderer, Color flashColor, float duration, int flashes)
    {
        Color originalColor = renderer.material.color;
        for (int i = 0; i < flashes; i++)
        {
            renderer.material.color = flashColor;
            yield return new WaitForSeconds(duration / 2);
            renderer.material.color = originalColor;
            yield return new WaitForSeconds(duration / 2);
        }
    }

    public void OnContainerTapped()
    {
        if (!gameStarted || robotHeldItem != null || currentItem == null || levelEnded) return;

        if (instructionShown && !instructionDismissed)
        {
            if (handInstructionCanvas != null)
                handInstructionCanvas.SetActive(false);

            instructionDismissed = true;
        }

        StartCoroutine(HandleRobotPickup());
    }

    IEnumerator HandleRobotPickup()
    {
        yield return RotateToAngle(90f);

        robotAnimator.SetBool("IsPickup", true);
        yield return new WaitForSeconds(pickupAnimationTime);
        robotAnimator.SetBool("IsPickup", false);

        robotHeldItem = currentItem;
        robotHeldItem.transform.SetParent(robot.transform);
        robotHeldItem.transform.position = itemHoldPoint.position;
        robotHeldItem.transform.localRotation = Quaternion.identity;
        robotHeldItem.transform.localScale = Vector3.one;
        currentItem = null;

        // 4. Turn back to face machine (180° on Y axis)
        yield return RotateToAngle(180f);
    }

    public void OnRedButtonPressed()
    {
        if (robotHeldItem == null || levelEnded) return;

        StartCoroutine(ProcessItem());
    }

    IEnumerator ProcessItem()
    {
        // 1. Play Place animation
        robotAnimator.SetBool("IsLetak", true);
        yield return new WaitForSeconds(placeAnimationTime);
        robotAnimator.SetBool("IsLetak", false);

        GameObject oldItem = robotHeldItem;
        robotHeldItem = null;

        // 2. Move item to machine input position
        oldItem.transform.SetParent(null);
        oldItem.transform.position = machineInputPoint.position;
        yield return new WaitForSeconds(1f);
        Destroy(oldItem);

        // 3. Create transformed item
        if (currentTransformedItem != null)
        {
            Destroy(currentTransformedItem);
        }

        if (currentContainerIndex >= 0 && currentContainerIndex < transformedItemPrefabs.Count)
        {
            currentTransformedItem = Instantiate(
                transformedItemPrefabs[currentContainerIndex],
                machineExitPoint.position,
                Quaternion.identity,
                containerParent
            );

            // Safety check
            if (currentContainerIndex >= 0 && currentContainerIndex < containerPrefabDetails.Count &&
                currentContainerIndex < transformedItemPrefabDetails.Count)
            {
                // Show item panel
                itemInfoPanel.SetActive(true);

                // Update old item info
                oldItemImage.sprite = containerPrefabDetails[currentContainerIndex].displayImage;
                oldItemText.text = containerPrefabDetails[currentContainerIndex].itemName;

                // Update new item info
                newItemImage.sprite = transformedItemPrefabDetails[currentContainerIndex].displayImage;
                newItemText.text = transformedItemPrefabDetails[currentContainerIndex].itemName;

                StartCoroutine(HideItemPanelAfterDelay(3f));
            }
        }
        else
        {
            Debug.LogWarning("Invalid container index for transformed item!");
        }

        // 4. Update game state
        completed++;
        AudioManager.instance?.PlaySFX("Yay");
        UpdateUI();

        if (completed >= totalToRecycle)
        {
            yield return new WaitForSeconds(2f);
            Destroy(currentTransformedItem);
            EndLevel();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            SpawnNewContainer();
        }

    }

    IEnumerator HideItemPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        itemInfoPanel.SetActive(false);
    }

    IEnumerator RotateToAngle(float targetYAngle)
    {
        float startYAngle = robot.transform.eulerAngles.y;
        float elapsed = 0f;

        // Handle angle wrapping
        float shortestAngle = Mathf.DeltaAngle(startYAngle, targetYAngle);
        targetYAngle = startYAngle + shortestAngle;

        while (elapsed < turnDuration)
        {
            float newYAngle = Mathf.Lerp(startYAngle, targetYAngle, elapsed / turnDuration);
            robot.transform.eulerAngles = new Vector3(
                robot.transform.eulerAngles.x,
                newYAngle,
                robot.transform.eulerAngles.z
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure exact final rotation
        robot.transform.eulerAngles = new Vector3(
            robot.transform.eulerAngles.x,
            targetYAngle,
            robot.transform.eulerAngles.z
        );
    }

    void UpdateUI()
    {
        completeText.text = $"{completed} / {totalToRecycle}";
    }

    void EndLevel()
    {
        if (levelEnded) return;

        levelEnded = true;
        gameStarted = false;

       
        AudioManager.instance?.musicSource.Stop();
        AudioManager.instance?.PlaySFX(completed >= totalToRecycle ? "Level Pass" : "Level Fail");
        

        int earnedStars = completed >= totalToRecycle ? 3 : (completed >= 3 ? 2 : 1);
        UpdateStarDisplay(earnedStars);

        if (endCanvas != null)
            endCanvas.SetActive(true);
    }

    void UpdateStarDisplay(int earnedStars)
    {
        for (int i = 0; i < stars.Length / 2; i++)
        {
            stars[i].SetActive(i < earnedStars);
            stars[i + stars.Length / 2].SetActive(i >= earnedStars);
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
