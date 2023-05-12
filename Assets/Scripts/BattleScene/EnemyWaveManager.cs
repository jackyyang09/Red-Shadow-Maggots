using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Facade;

public class EnemyWaveManager : BasicSingleton<EnemyWaveManager>
{
    [SerializeField] int waveCount = 0;
    public int WaveCount 
    { 
        get
        { 
            if (playerDataManager.LoadedData.InBattle) return battleStateManager.LoadedData.WaveCount;
            return waveCount;
        } 
        set 
        {
            if (playerDataManager.LoadedData.InBattle) battleStateManager.LoadedData.WaveCount = value;
            else waveCount = value; 
        } 
    }

    public bool IsLastWave 
    { 
        get 
        {
            if (playerDataManager.LoadedData.InBattle)
            {
                return WaveCount == battleStateManager.LoadedData.EnemyGUIDs.Count - 1;
            }
            else return waveCount == waves.Length - 1; 
        } 
    }

    public WaveObject CurrentWave { get { return waves[waveCount]; } }

    public int TotalWaves
    { 
        get 
        {
            if (playerDataManager.LoadedData.InBattle)
            {
                return battleStateManager.LoadedData.EnemyGUIDs.Count;
            }
            return waves.Length; 
        } 
    }

    [SerializeField] WaveObject[] waves = null;

    public static System.Action OnEnterBossWave;

    public void SetupWave()
    {
        RemoveDeadEnemies();

        if (PlayerData.InBattle)
        {
            characterLoader.LoadAllEnemies();
        }
        else
        {
            characterLoader.LoadAllEnemies(waves[waveCount]);
        }
    }

    public void RemoveDeadEnemies()
    {
        if (enemyController.Enemies == null) return;
        for (int i = 0; i < enemyController.Enemies.Length; i++)
        {
            if (!enemyController.Enemies[i]) continue;
            Destroy(enemyController.Enemies[i].gameObject);
        }
    }
}