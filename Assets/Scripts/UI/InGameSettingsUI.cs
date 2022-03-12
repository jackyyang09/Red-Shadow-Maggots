using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JSAM;

public class InGameSettingsUI : MonoBehaviour
{
    [SerializeField] Slider masterSlider = null;
    [SerializeField] Slider musicSlider = null;
    [SerializeField] Slider soundSlider = null;

    private void OnEnable()
    {
        AudioManager.OnAudioManagerInitialized += UpdateAllSliders;
        AudioManager.OnMasterVolumeChanged += UpdateMaster;
        AudioManager.OnMusicVolumeChanged += UpdateMusic;
        AudioManager.OnSoundVolumeChanged += UpdateSound;
    }

    private void OnDisable()
    {
        AudioManager.OnAudioManagerInitialized -= UpdateAllSliders;
        AudioManager.OnMasterVolumeChanged -= UpdateMaster;
        AudioManager.OnMusicVolumeChanged -= UpdateMusic;
        AudioManager.OnSoundVolumeChanged -= UpdateSound;
    }

    void UpdateMaster(float vol) => masterSlider.value = vol;
    void UpdateMusic(float vol) => musicSlider.value = vol;
    void UpdateSound(float vol) => soundSlider.value = vol;

    void UpdateAllSliders()
    {
        UpdateMaster(AudioManager.MasterVolume);
        UpdateMusic(AudioManager.MusicVolume);
        UpdateSound(AudioManager.SoundVolume);
    }
}