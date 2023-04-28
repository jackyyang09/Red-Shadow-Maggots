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

    Dictionary<AsyncOperationHandle<CharacterObject>, int> playerHandleToIndex = new Dictionary<AsyncOperationHandle<CharacterObject>, int>();
    Dictionary<AsyncOperationHandle<CharacterObject>, int> enemyHandleToIndex = new Dictionary<AsyncOperationHandle<CharacterObject>, int>();

    EnemyCharacter[] enemies = new EnemyCharacter[3];

    public void LoadAllPlayerCharacters()
    {
        playersLoaded = 0;
        playerHandleToIndex.Clear();

        battleSystem.PlayerCharacters.AddRange(new PlayerCharacter[3]);

        for (int i = 0; i < PlayerData.Party.Length; i++)
        {
            if (PlayerData.Party[i] == -1)
            {
                continue;
            }

            var guid = PlayerData.MaggotStates[PlayerData.Party[i]].GUID;
            var opHandle = Addressables.LoadAssetAsync<CharacterObject>(guid);
            opHandle.Completed += OnCharacterLoaded;
            playerHandleToIndex.Add(opHandle, i);
        }
    }

    private void OnCharacterLoaded(AsyncOperationHandle<CharacterObject> obj)
    {
        var i = playerHandleToIndex[obj];
        var mState = PlayerData.MaggotStates[PlayerData.Party[i]];
        var characterObject = obj.Result;
        var pState = BattleData.PlayerStates.Count > 0 ? BattleData.PlayerStates[i] : null;
        var level = characterObject.GetLevelFromExp(mState.Exp);

        characterLoader.SpawnCharacterWithRarity(i, characterObject, Rarity.Common, level, pState);
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

        for (int j = 0; j < currentWave.Count; j++)
        {
            string guid = currentWave[j];

            if (!guid.IsNullEmptyOrWhiteSpace())
            {
                var opHandle = Addressables.LoadAssetAsync<CharacterObject>(currentWave[j]);
                opHandle.Completed += OnEnemyLoaded;
                enemyHandleToIndex.Add(opHandle, j);
            }
        }
    }

    private void OnEnemyLoaded(AsyncOperationHandle<CharacterObject> obj)
    {
        var j = enemyHandleToIndex[obj];
        var t = enemySpawns[j];
        switch (j)
        {
            case 1:
                if (BattleData.IsBossWave[BattleData.WaveCount])
                {
                    var s = BattleData.EnemyStates.Count > 0 ? BattleData.EnemyStates[j] : null;
                    enemies[j] = SpawnBoss(obj.Result, enemySpawns[1], BattleData.RoomLevel, s);
                }
                break;
        }

        BattleState.EnemyState state = BattleData.EnemyStates.Count > 0 ? BattleData.EnemyStates[j] : null;
        enemies[j] = SpawnEnemy(obj.Result, t, BattleData.RoomLevel, state);
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
            Addressables.Release(item.Key);
        }

        foreach (var item in enemyHandleToIndex)
        {
            Addressables.Release(item.Key);
        }
    }
}