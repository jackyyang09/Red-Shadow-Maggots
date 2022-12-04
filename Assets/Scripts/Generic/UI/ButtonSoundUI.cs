using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JSAM;

[RequireComponent(typeof(Button))]
public class ButtonSoundUI : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] JSAMSoundFileObject buttonSound;

    private void OnValidate()
    {
        if (!button) button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(PlayButtonSound);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(PlayButtonSound);
    }

    public void PlayButtonSound() => AudioManager.PlaySound(buttonSound);
}