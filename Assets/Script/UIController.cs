using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Slider _musicSlider, _sfxSlider;

    private void Start()
    {
        // Sync sliders to current audio levels
        if (AudioManager.instance != null)
        {
            _musicSlider.value = AudioManager.instance.musicSource.volume;
            _sfxSlider.value = AudioManager.instance.sfxSource.volume;
        }

        // Add listeners if not using OnValueChanged from Inspector
        _musicSlider.onValueChanged.AddListener(delegate { MusicVolume(); });
        _sfxSlider.onValueChanged.AddListener(delegate { SFXvolume(); });
    }

    public void ToggleMusic()
    {
        AudioManager.instance.ToggleMusic();
    }

    public void ToggleSFX()
    {
        AudioManager.instance.ToggleSFX();
    }

    public void MusicVolume()
    {
        AudioManager.instance.MusicVolume(_musicSlider.value);
    }

    public void SFXvolume()
    {
        AudioManager.instance.SFXVolume(_sfxSlider.value);
    }
}
