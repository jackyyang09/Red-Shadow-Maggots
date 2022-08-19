using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement;
using static Facade;

public class RestNode : BasicSingleton<RestNode>
{
    [SerializeField] float healPercentage = 0.5f;
    [SerializeField] float fadeTime = 1;
    [SerializeField] Cinemachine.CinemachineVirtualCamera vCam;
    void DisableVirtualCam() => vCam.enabled = false;
    [SerializeField] OptimizedCanvas restCanvas;
    [SerializeField] GameObject cardCanvas;

    public void Initialize()
    {
        restCanvas.Show();
        vCam.enabled = true;
    }

    public void Upgrade()
    {
        cardListUI.ShowUI();
        restCanvas.Hide();
    }

    public void FinishUpgrade()
    {
        Invoke(nameof(DisableVirtualCam), 0.5f);
    }

    public void Rest()
    {
        StartCoroutine(RestRoutine());
    }

    IEnumerator RestRoutine()
    {
        restCanvas.Hide();

        var data = playerDataManager.LoadedData;
        var states = data.MaggotStates;

        for (int i = 0; i < states.Count; i++)
        {
            var op = gachaSystem.LoadMaggot(gachaSystem.GUIDToAssetReference[states[i].GUID]);

            yield return op;

            var character = op.Result as CharacterObject;

            var level = 1 + states[i].Exp % 100;
            float maxHealth = (float)character.GetMaxHealth((int)level, false);
            states[i].Health = Mathf.Clamp(
                states[i].Health + maxHealth * healPercentage,
                0, maxHealth);
        }

        battleStateManager.SaveData();

        float halfTime = fadeTime / 2;

        yield return screenEffects.FadeToWhiteRoutine(halfTime);
        yield return screenEffects.FadeFromWhiteRoutine(halfTime);

        vCam.enabled = false;
    }
}