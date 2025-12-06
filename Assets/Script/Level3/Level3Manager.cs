using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Level3Manager : MonoBehaviour
{
    [System.Serializable]
    public class QuizQuestion
    {
        public string question;
        public string[] options = new string[3];
        public int correctIndex;
    }

    [Header("Quiz Data")]
    public List<QuizQuestion> questions;
    private int currentQuestionIndex = -1;
    private int correct = 0;
    private int maxCorrect = 5;
    private float questionTimer = 60f;

    [Header("UI Elements")]
    public TMP_Text questionText;
    public TMP_Text timerText;
    public TMP_Text[] answerTexts;
    public TMP_Text correctAnswerText;
    public GameObject[] answerCanvases;

    [Header("Health")]
    public GameObject[] hearts; // 0-2 red, 3-5 black
    private int health = 3;
    private int maxHealth = 3;

    [Header("Game Control")]
    public GameObject endCanvas;
    public GameObject[] stars;
    public GameObject settingsPanel;
    public GameObject instructionCanvas;
    private bool isPaused = false;
    private float previousTimeScale;
    private bool levelEnded = false;
    private bool questionActive = false;

    [Header("Animator")]
    public Animator quizAnimator; // Drag your Animator here in Inspector

    [Header("Hand Canvas")]
    public GameObject handInstructionCanvas;
    private bool instructionShown = false;
    private bool instructionDismissed = false;

    void Start()
    {
        ShuffleQuestions();
        ShowNextQuestion();
        UpdateCorrectAnswerText();
        instructionCanvas.SetActive(false);

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
        if (!levelEnded && questionActive)
        {
            questionTimer -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(questionTimer).ToString();

            if (questionTimer <= 0f)
            {
                HandleAnswer(-1); // Timeout counts as wrong
            }
        }

        if (instructionShown && !instructionDismissed)
        {
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0)) // Touch or mouse click
            {
                if (handInstructionCanvas != null)
                    handInstructionCanvas.SetActive(false);

                instructionDismissed = true;
            }
        }
    }

    void ShuffleQuestions()
    {
        for (int i = 0; i < questions.Count; i++)
        {
            QuizQuestion temp = questions[i];
            int rand = Random.Range(i, questions.Count);
            questions[i] = questions[rand];
            questions[rand] = temp;
        }
    }

    void ShowNextQuestion()
    {
        currentQuestionIndex++;
        if (correct >= maxCorrect || health <= 0 || currentQuestionIndex >= questions.Count)
        {
            EndLevel();
            return;
        }

        questionTimer = 60f;
        questionActive = true;

        QuizQuestion q = questions[currentQuestionIndex];
        StartCoroutine(TypeQuestion(q.question));

        for (int i = 0; i < answerTexts.Length; i++)
        {
            answerTexts[i].text = q.options[i];
        }
    }

    IEnumerator TypeQuestion(string text)
    {
        questionText.text = "";
        foreach (char c in text)
        {
            questionText.text += c;
            yield return new WaitForSeconds(0.03f); // Typewriter speed
        }
    }

    public void HandleAnswer(int selectedIndex)
    {
        
        if (!questionActive || levelEnded) return;
        questionActive = false;

        QuizQuestion q = questions[currentQuestionIndex];
        if (selectedIndex == q.correctIndex)
        {
            correct++;
            AudioManager.instance?.PlaySFX("Correct");
            quizAnimator?.SetBool("IsBetul", true);
            UpdateCorrectAnswerText();
        }
        else
        {
            health = Mathf.Max(0, health - 1);
            AudioManager.instance?.PlaySFX("Wrong");
            quizAnimator?.SetBool("IsSalah", true);
            UpdateHealthDisplay();
        }
        Invoke(nameof(ResetQuizAnimationBools), 0.5f);
        Invoke(nameof(ShowNextQuestion), 1f); // Delay before next question
    }

    void ResetQuizAnimationBools()
    {
        quizAnimator?.SetBool("IsBetul", false);
        quizAnimator?.SetBool("IsSalah", false);
    }

    void UpdateHealthDisplay()
    {
        for (int i = 0; i < maxHealth; i++)
        {
            if (hearts.Length > i + maxHealth)
            {
                hearts[i].SetActive(i < health);             // Red
                hearts[i + maxHealth].SetActive(i >= health); // Black
            }
        }
    }

    void UpdateCorrectAnswerText()
    {
        correctAnswerText.text = $"{correct} / {maxCorrect}";
    }

    void EndLevel()
    {
        levelEnded = true;
        questionActive = false;
        ShowEndCanvas();
    }

    void ShowEndCanvas()
    {
        int earnedStars = (correct >= maxCorrect) ? Mathf.Clamp(health, 1, 3) : 0;
        UpdateStarDisplay(earnedStars);
        AudioManager.instance?.musicSource.Stop();
        AudioManager.instance?.PlaySFX(earnedStars > 0 ? "Level Pass" : "Level Fail");
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

    #region Pause Controls
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

    void OnDisable()
    {
        if (isPaused) ResumeGame();
    }

    public void RestartLevel()
    {
        ResetTimeScale();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        AudioManager.instance?.musicSource.Play();
    } 
    public void GoHome()
    {
        ResetTimeScale();
        SceneManager.LoadScene("Main Menu");
        AudioManager.instance?.musicSource.Play();
    }
 
    public void Nextlevel()
    {
        ResetTimeScale();
        SceneManager.LoadScene("AR");
        AudioManager.instance?.musicSource.Play();
    }

    void ResetTimeScale()
    {
        Time.timeScale = 1f;
    }
    #endregion
}
