using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioButton : MonoBehaviour
{
    [SerializeField] private string sfxName;

    // Called by the UI Button OnClick() in the Inspector
    public void PlaySFX()
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError("AudioManager.instance is null! Make sure AudioManager exists in your first scene.");
            return;
        }
        if (string.IsNullOrEmpty(sfxName))
        {
            Debug.LogWarning($"No sfxName set on {gameObject.name}");
            return;
        }

        AudioManager.instance.PlaySFX(sfxName);
    }
}

