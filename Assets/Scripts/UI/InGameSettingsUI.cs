using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JSAM;
using static Facade;

public class InGameSettingsUI : MonoBehaviour
{
    [SerializeField] OptimizedCanvas[] uiCanvases = null;

    [Header("Graphics")]
    [SerializeField] Slider qualitySlider = null;
    [SerializeField] Toggle postProcessingToggle = null;

    [Header("Volume")]
    [SerializeField] Slider masterSlider = null;
    [SerializeField] Slider musicSlider = null;
    [SerializeField] Slider soundSlider = null;

    private void OnEnable()
    {
        AudioManager.OnAudioManagerInitialized += UpdateVolumeSliders;
        AudioManager.OnMasterVolumeChanged += UpdateMaster;
        AudioManager.OnMusicVolumeChanged += UpdateMusic;
        AudioManager.OnSoundVolumeChanged += UpdateSound;

        qualitySlider.value = QualitySettings.GetQualityLevel();
    }

    private void OnDisable()
    {
        AudioManager.OnAudioManagerInitialized -= UpdateVolumeSliders;
        AudioManager.OnMasterVolumeChanged -= UpdateMaster;
        AudioManager.OnMusicVolumeChanged -= UpdateMusic;
        AudioManager.OnSoundVolumeChanged -= UpdateSound;
    }

    public void HideAllUIExcept(int ignoreIndex = -1)
    {
        for (int i = 0; i < uiCanvases.Length; i++)
        {
            uiCanvases[i].SetActive(i == ignoreIndex);
        }
    }

    public void TogglePostProcessing()
    {
        graphicsSettings.PostProcessing.enabled = !graphicsSettings.PostProcessing.enabled;
    }

    public void ShowGraphicsUI()
    {
        HideAllUIExcept(0);
    }
    
    public void ShowVolumeUI()
    {
        HideAllUIExcept(1);
    }

    void UpdateMaster(float vol) => masterSlider.value = vol;
    void UpdateMusic(float vol) => musicSlider.value = vol;
    void UpdateSound(float vol) => soundSlider.value = vol;

    void UpdateVolumeSliders()
    {
        UpdateMaster(AudioManager.MasterVolume);
        UpdateMusic(AudioManager.MusicVolume);
        UpdateSound(AudioManager.SoundVolume);
    }
}