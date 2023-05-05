using System;
using System.Collections;
using System.Collections.Generic;
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

    public bool IsBossWave
    {
        get
        {
            if (playerDataManager.LoadedData.InBattle)
            {
                return battleStateManager.LoadedData.IsBossWave[battleStateManager.LoadedData.WaveCount];
            }
            return CurrentWave.IsBossWave;
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

            if (BattleData.IsBossWave[BattleData.WaveCount]) OnEnterBossWave?.Invoke();
        }
        else
        {
            WaveObject newWave = waves[waveCount];
            var enemies = new List<EnemyCharacter>(new EnemyCharacter[3]);

            if (newWave.leftEnemy)
            {
                enemies[0] = characterLoader.SpawnEnemy(newWave.leftEnemy, leftSpawnPos);
            }

            if (newWave.middleEnemy)
            {
                if (newWave.IsBossWave)
                {
                    enemies[1] = characterLoader.SpawnBoss(newWave.middleEnemy, middleSpawnPos);
                }
                else
                {
                    enemies[1] = characterLoader.SpawnEnemy(newWave.middleEnemy, middleSpawnPos);
                }
            }

            if (newWave.rightEnemy)
            {
                enemies[2] = characterLoader.SpawnEnemy(newWave.rightEnemy, rightSpawnPos);
            }

            if (newWave.IsBossWave) OnEnterBossWave?.Invoke();

            enemyController.AssignEnemies(enemies.ToArray());
        }
    }

    public void RemoveDeadEnemies()
    {
        if (leftSpawnPos.childCount > 0) Destroy(leftSpawnPos.GetChild(0).gameObject);
        if (middleSpawnPos.childCount > 0) Destroy(middleSpawnPos.GetChild(0).gameObject);
        if (rightSpawnPos.childCount > 0) Destroy(rightSpawnPos.GetChild(0).gameObject);
    }
}