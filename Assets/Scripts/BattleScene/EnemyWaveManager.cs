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

    [SerializeField] Transform leftSpawnPos = null;

    [SerializeField] Transform middleSpawnPos = null;

    [SerializeField] Transform rightSpawnPos = null;

    [SerializeField] GameObject enemyPrefab = null;

    [SerializeField] GameObject bossPrefab = null;

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
        if (leftSpawnPos.childCount > 0) Destroy(leftSpawnPos.GetChild(0).gameObject);
        if (middleSpawnPos.childCount > 0) Destroy(middleSpawnPos.GetChild(0).gameObject);
        if (rightSpawnPos.childCount > 0) Destroy(rightSpawnPos.GetChild(0).gameObject);
    }
}