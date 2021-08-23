using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JSAM;

public class InGameSettingsUI : MonoBehaviour
{
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider soundSlider;

    public void UpdateSliderValues()
    {
        //masterSlider.value = AudioManager.GetMasterVolume();
        //musicSlider.value = AudioManager.GetMusicVolume();
        //soundSlider.value = AudioManager.GetSoundVolume();
    }
}
