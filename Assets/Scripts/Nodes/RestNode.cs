﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        restCanvas.Hide();
        cardListUI.InitializeAsUpgradeUI();
        cardListUI.OnBackOut += OnBackOut;
    }

    public void OnBackOut()
    {
        restCanvas.Show();
        cardListUI.OnBackOut -= OnBackOut;
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
            var op = Addressables.LoadAssetAsync<CharacterObject>(states[i].GUID);

            yield return op;

            var character = op.Result;

            var level = character.GetLevelFromExp(states[i].Exp);
            float maxHealth = character.GetMaxHealth(level, false);
            states[i].Health = Mathf.Clamp(
                states[i].Health + maxHealth * healPercentage,
                0, maxHealth);
        }

        mapManager.SaveMap();
        battleStateManager.SaveData();

        float halfTime = fadeTime / 2;

        yield return screenEffects.FadeToWhiteRoutine(ScreenEffects.EffectType.Partial, halfTime);
        yield return screenEffects.FadeFromWhiteRoutine(ScreenEffects.EffectType.Partial, halfTime);

        vCam.enabled = false;
    }
}