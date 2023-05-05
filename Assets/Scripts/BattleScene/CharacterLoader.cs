using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Facade;

public class CharacterLoader : BasicSingleton<CharacterLoader>
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject bossPrefab;

    [SerializeField] Transform[] playerSpawns;
    [SerializeField] Transform[] enemySpawns;

    int playersLoaded, enemiesLoaded;
    public bool PlayersLoaded => playersLoaded == playerHandleToIndex.Count;
    public bool EnemiesLoaded => enemiesLoaded == enemyHandleToIndex.Count;

    List<AsyncOperationHandle<CharacterObject>> playerHandleToIndex = new List<AsyncOperationHandle<CharacterObject>>();
    List<AsyncOperationHandle<CharacterObject>> enemyHandleToIndex = new List<AsyncOperationHandle<CharacterObject>>();

    EnemyCharacter[] enemies = new EnemyCharacter[3];

    public void LoadAllPlayerCharacters()
    {
        playersLoaded = 0;
        playerHandleToIndex.Clear();

        for (int i = 0; i < PlayerData.Party.Length; i++)
        {
            if (PlayerData.Party[i] == -1)
            {
                continue;
            }

            var guid = PlayerData.MaggotStates[PlayerData.Party[i]].GUID;
            var opHandle = Addressables.LoadAssetAsync<CharacterObject>(guid);
            StartCoroutine(LoadCharacter(i, opHandle));
            playerHandleToIndex.Add(opHandle);
        }
    }

    IEnumerator LoadCharacter(int index, AsyncOperationHandle<CharacterObject> obj)
    {
        yield return obj;

        var mState = PlayerData.MaggotStates[PlayerData.Party[index]];
        var characterObject = obj.Result;
        var pState = BattleData.PlayerStates.Count > 0 ? BattleData.PlayerStates[index] : null;
        var level = characterObject.GetLevelFromExp(mState.Exp);

        characterLoader.SpawnCharacterWithRarity(index, characterObject, Rarity.Common, level, pState);
        playersLoaded++;
    }

    public void SpawnCharacterWithRarity(int index, CharacterObject character, Rarity rarity, int level = 1,
    BattleState.PlayerState stateInfo = null)
    {
        int playerCount = battleSystem.PlayerCharacters.Count(item => item != null);
        Transform spawnPos = playerSpawns[playerCount];

        PlayerCharacter player = Instantiate(playerPrefab, spawnPos).GetComponent<PlayerCharacter>();
        player.SetCharacterAndRarity(character, rarity);
        player.InitializeWithInfo(level, stateInfo);
        battleSystem.PlayerCharacters[index] = player;
    }

    public void LoadAllEnemies()
    {
        enemiesLoaded = 0;

        var currentWave = BattleData.EnemyGUIDs[BattleData.WaveCount];

        for (int i = 0; i < currentWave.Count; i++)
        {
            string guid = currentWave[i];

            if (!guid.IsNullEmptyOrWhiteSpace())
            {
                var opHandle = Addressables.LoadAssetAsync<CharacterObject>(currentWave[i]);
                StartCoroutine(LoadEnemy(i, opHandle));
                enemyHandleToIndex.Add(opHandle);
            }
        }
    }

    IEnumerator LoadEnemy(int index, AsyncOperationHandle<CharacterObject> obj)
    {
        yield return obj;

        var t = enemySpawns[index];
        switch (index)
        {
            case 1:
                if (BattleData.IsBossWave[BattleData.WaveCount])
                {
                    var s = BattleData.EnemyStates.Count > 0 ? BattleData.EnemyStates[index] : null;
                    enemies[index] = SpawnBoss(obj.Result, enemySpawns[1], BattleData.RoomLevel, s);
                }
                break;
        }

        BattleState.EnemyState state = BattleData.EnemyStates.Count > 0 ? BattleData.EnemyStates[index] : null;
        enemies[index] = SpawnEnemy(obj.Result, t, BattleData.RoomLevel, state);
        enemiesLoaded++;

        if (EnemiesLoaded)
        {
            enemyController.AssignEnemies(enemies);
        }
    }

    public EnemyCharacter SpawnEnemy(CharacterObject character, Transform spawnPos, int level = 1, BattleState.EnemyState stateInfo = null)
    {
        EnemyCharacter newEnemy = Instantiate(enemyPrefab.gameObject, spawnPos).GetComponent<EnemyCharacter>();
        newEnemy.SetCharacterAndRarity(character, Rarity.Common);
        newEnemy.InitializeWithInfo(level, stateInfo);

        return newEnemy;
    }

    public EnemyCharacter SpawnBoss(CharacterObject character, Transform spawnPos, int level = 1, BattleState.EnemyState stateInfo = null)
    {
        EnemyCharacter newEnemy = Instantiate(bossPrefab.gameObject, spawnPos).GetComponent<EnemyCharacter>();
        newEnemy.SetCharacterAndRarity(character, Rarity.Common);
        newEnemy.InitializeWithInfo(level, stateInfo);

        return newEnemy;
    }

    private void OnDestroy()
    {
        foreach (var item in playerHandleToIndex)
        {
            Addressables.Release(item);
        }

        foreach (var item in enemyHandleToIndex)
        {
            Addressables.Release(item);
        }
    }
}