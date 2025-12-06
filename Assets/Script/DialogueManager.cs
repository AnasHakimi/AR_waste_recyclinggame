using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;

    public AudioSource voiceAudioSource;
    public AudioSource sfxAudioSource;

    public float typingSpeed = 0.05f;

    public Button nextButton;
    public Button prevButton;
    public Button audioButton;
    public Button skipButton;

    [TextArea(2, 5)]
    public List<string> dialogueLines;
    public List<AudioClip> voiceClips;

    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;

    public string mainMenuSceneName;

    public AudioClip buttonSFX;
    void Start()
    {
        // NEXT BUTTON
        nextButton.onClick.AddListener(() => {
            Animationbttn(nextButton);  
            NextLine();                 
        });

        // PREVIOUS BUTTON
        prevButton.onClick.AddListener(() => {
            Animationbttn(prevButton);  
            PrevLine();                 
        });

        // AUDIO BUTTON
        audioButton.onClick.AddListener(() => {
            Animationbttn(audioButton); 
            PlayVoiceClip();            
        });

        // SKIP BUTTON
        skipButton.onClick.AddListener(() => {
            Animationbttn(skipButton); 
            SkipIntro();

        });

        
        ShowLine(currentLineIndex);
    }

    void Awake()
    {
        if (PlayerPrefs.GetInt("IntroPlayed", 0) == 2) // nanti tukar jadi 1 (buat 2 sbb nak test je)
        {
            SceneManager.LoadScene(mainMenuSceneName); // skip intro if already played
        }
    }

    void ShowLine(int index)
    {
        if (index >= 0 && index < dialogueLines.Count)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText(dialogueLines[index]));

            // Stop voice-over if it was playing
            if (voiceAudioSource.isPlaying)
                voiceAudioSource.Stop();
        }
    }

    IEnumerator TypeText(string line)
    {
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }


    public void Animationbttn(Button button)
    {
        button.GetComponent<Animation>()?.Play("buttonAni");

        if (buttonSFX != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(buttonSFX);
        }

    }


    public void NextLine()
    {
        if (currentLineIndex < dialogueLines.Count - 1)
        {
            currentLineIndex++;
            ShowLine(currentLineIndex);
        }
    }

    public void PrevLine()
    {
        if (currentLineIndex > 0)
        {
            currentLineIndex--;
            ShowLine(currentLineIndex);
        }
    }

    public void PlayVoiceClip()
    {
        if (currentLineIndex < voiceClips.Count && voiceClips[currentLineIndex] != null)
        {
            voiceAudioSource.Stop(); 
            voiceAudioSource.PlayOneShot(voiceClips[currentLineIndex]);
        }
    }

    public void SkipIntro()
    {
        PlayerPrefs.SetInt("IntroPlayed", 1); 
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
