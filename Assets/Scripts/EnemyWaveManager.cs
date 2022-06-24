using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

    public IEnumerator SetupWave()
    {
        RemoveDeadEnemies();

        var enemies = new List<EnemyCharacter>();

        if (playerDataManager.LoadedData.InBattle)
        {
            var data = battleStateManager.LoadedData;
            var currentWave = data.EnemyGUIDs[data.WaveCount];
            for (int j = 0; j < currentWave.Count; j++)
            {
                string guid = currentWave[j];

                if (!guid.IsNullEmptyOrWhiteSpace())
                {
                    var opHandle = Addressables.LoadAssetAsync<CharacterObject>(currentWave[j]);
                    yield return opHandle;

                    if (opHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded &&
                        opHandle.Result != null)
                    {
                        var enemy = opHandle.Result;
                        var t = leftSpawnPos;
                        switch (j)
                        {
                            case 1:
                                if (data.IsBossWave[data.WaveCount])
                                {
                                    var s = data.EnemyStates.Count > 0 ? data.EnemyStates[j] : null;
                                    enemies.Add(SpawnBoss(enemy, middleSpawnPos, data.RoomLevel, s));
                                    continue;
                                }
                                t = middleSpawnPos;
                                break;
                            case 2:
                                t = rightSpawnPos;
                                break;
                        }

                        BattleState.EnemyState state = data.EnemyStates.Count > 0 ? data.EnemyStates[j] : null;
                        enemies.Add(SpawnEnemy(enemy, t, data.RoomLevel, state));
                    }
                }
                else
                {
                    enemies.Add(null);
                }
            }

            if (data.IsBossWave[data.WaveCount]) OnEnterBossWave?.Invoke();
        }
        else
        {
            WaveObject newWave = waves[waveCount];

            if (newWave.leftEnemy)
            {
                enemies.Add(SpawnEnemy(newWave.leftEnemy, leftSpawnPos));
            }

            if (newWave.middleEnemy)
            {
                if (newWave.IsBossWave)
                {
                    enemies.Add(SpawnBoss(newWave.middleEnemy, middleSpawnPos));
                }
                else
                {
                    enemies.Add(SpawnEnemy(newWave.middleEnemy, middleSpawnPos));
                }
            }

            if (newWave.rightEnemy)
            {
                enemies.Add(SpawnEnemy(newWave.rightEnemy, rightSpawnPos));
            }

            if (newWave.IsBossWave) OnEnterBossWave?.Invoke();
        }

        enemyController.AssignEnemies(enemies);
    }

    public void RemoveDeadEnemies()
    {
        if (leftSpawnPos.childCount > 0) Destroy(leftSpawnPos.GetChild(0).gameObject);
        if (middleSpawnPos.childCount > 0) Destroy(middleSpawnPos.GetChild(0).gameObject);
        if (rightSpawnPos.childCount > 0) Destroy(rightSpawnPos.GetChild(0).gameObject);
    }

    public EnemyCharacter SpawnEnemy(CharacterObject character, Transform spawnPos, int level = 1, BattleState.EnemyState stateInfo = null)
    {
        EnemyCharacter newEnemy = Instantiate(enemyPrefab.gameObject, spawnPos).GetComponent<EnemyCharacter>();
        newEnemy.SetCharacterAndRarity(character, Rarity.Common);
        newEnemy.ApplyCharacterStats(level, stateInfo);

        return newEnemy;
    }

    public EnemyCharacter SpawnBoss(CharacterObject character, Transform spawnPos, int level = 1, BattleState.EnemyState stateInfo = null)
    {
        EnemyCharacter newEnemy = Instantiate(bossPrefab.gameObject, spawnPos).GetComponent<EnemyCharacter>();
        newEnemy.SetCharacterAndRarity(character, Rarity.Common);
        newEnemy.ApplyCharacterStats(level, stateInfo);

        return newEnemy;
    }
}
