using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JSAM;
using static Facade;

public class InGameSettingsUI : BaseGameUI
{
    [SerializeField] OptimizedCanvas[] uiCanvases;

    [Header("Graphics")]
    [SerializeField] Slider qualitySlider;
    [SerializeField] Toggle postProcessingToggle;
    [SerializeField] Toggle antiAliasingToggle;

    [Header("Volume")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider soundSlider;
    [SerializeField] Slider voiceSlider;

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

    public override void ShowUI()
    {
        optimizedCanvas.Show();
        playerControlManager.SetControlMode(PlayerControlMode.InSettings);
    }

    public override void HideUI()
    {
        optimizedCanvas.Hide();
        playerControlManager.ReturnControl();
    }

    public void HideAllUIExcept(int ignoreIndex = -1)
    {
        for (int i = 0; i < uiCanvases.Length; i++)
        {
            //uiCanvases[i].SetActive(i == ignoreIndex);
            uiCanvases[i].gameObject.SetActive(i == ignoreIndex);
        }
    }

    public void TogglePostProcessing()
    {
        graphicsSettings.PostProcessing.enabled = !graphicsSettings.PostProcessing.enabled;
    }

    public void ToggleAntiAliasing()
    {
        switch (graphicsSettings.PostProcessing.antialiasingMode)
        {
            case UnityEngine.Rendering.PostProcessing.PostProcessLayer.Antialiasing.None:
            graphicsSettings.PostProcessing.antialiasingMode = UnityEngine.Rendering.PostProcessing.PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                break;
            case UnityEngine.Rendering.PostProcessing.PostProcessLayer.Antialiasing.FastApproximateAntialiasing:
            graphicsSettings.PostProcessing.antialiasingMode = UnityEngine.Rendering.PostProcessing.PostProcessLayer.Antialiasing.None;
                break;
        }
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
    void UpdateVoice(float vol) => voiceSlider.value = vol;

    void UpdateVolumeSliders()
    {
        UpdateMaster(AudioManager.MasterVolume);
        UpdateMusic(AudioManager.MusicVolume);
        UpdateSound(AudioManager.SoundVolume);
        UpdateVoice(AudioManager.VoiceVolume);
    }
}