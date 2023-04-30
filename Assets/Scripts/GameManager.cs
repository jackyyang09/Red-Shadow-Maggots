﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class GameManager : BasicSingleton<GameManager>
{
    [SerializeField] int baseExpGain = 100;

    public int ExpGained
    {
        get
        {
            var data = battleStateManager.LoadedData;
            var GUIDs = data.EnemyGUIDs;

            int enemyCount = 0;
            for (int i = 0; i < GUIDs.Count; i++)
            {
                for (int j = 0; j < GUIDs[i].Count; j++)
                {
                    if (!string.IsNullOrEmpty(GUIDs[i][j])) enemyCount++;
                }
            }
            return baseExpGain * enemyCount;
        }
    }

    int turnCount = 0;
    public int TurnCount
    {
        get { return turnCount; }
        set
        {
            turnCount = value;
            OnTurnCountChanged?.Invoke();
        }
    }
    private void IncrementTurnCount() => TurnCount++;

    [SerializeField] DevLocker.Utils.SceneReference mapScene;

    public static System.Action OnTurnCountChanged;

    private void OnEnable()
    {
        BattleSystem.OnEndTurn += IncrementTurnCount;
    }

    private void OnDisable()
    {
        BattleSystem.OnEndTurn -= IncrementTurnCount;
    }

    // Start is called before the first frame update
    void Start()
    {
        DiscordWrapper.Instance.UpdateActivity(
            state: "In-Battle",
            largeImageKey: "morshu",
            partySize: waveManager.WaveCount + 1,
            partyMax: waveManager.TotalWaves,
            startTime: DiscordWrapper.CurrentTime
            );
    }

    public void ReturnToMapVictorious()
    {
        battleStateManager.DeleteData();

        PlayerData.Exp += ExpGained;
        PlayerData.InBattle = false;
        PlayerData.BattlesFought++;

        for (int i = 0; i < battleSystem.PlayerCharacters.Count; i++)
        {
            if (!battleSystem.PlayerCharacters[i]) continue;
            PlayerData.MaggotStates[i].Health = battleSystem.PlayerCharacters[i].CurrentHealth;
        }

        playerDataManager.SaveData();

        sceneLoader.SwitchScene(mapScene.SceneName);
    }

    public void RestartBattle()
    {
        sceneLoader.ReloadScene();
    }
}