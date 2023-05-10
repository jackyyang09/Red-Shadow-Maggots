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
    public Vector3 GetPlayerSpawns(float lerp) => Vector3.Lerp(playerSpawns.First().position, playerSpawns.Last().position, lerp);
    [SerializeField] Transform[] enemySpawns;
    public Vector3 GetEnemySpawns(float lerp) => Vector3.Lerp(enemySpawns.First().position, enemySpawns.Last().position, lerp);

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

        float partySize = PlayerData.Party.Where(t => t > -1).Count();
        float padding = 0;
        if (partySize % 2 == 0)
        {
            padding = 0.5f / partySize;
        }
        else if (partySize == 1) padding = 0.5f;

        for (int i = 0; i < PlayerData.Party.Length; i++)
        {
            if (PlayerData.Party[i] == -1)
            {
                continue;
            }

            var spawnPos = GetPlayerSpawns(padding + (float)i / partySize);
            var guid = PlayerData.MaggotStates[PlayerData.Party[i]].GUID;
            var opHandle = Addressables.LoadAssetAsync<CharacterObject>(guid);
            StartCoroutine(LoadCharacter(i, spawnPos, opHandle));
            playerHandleToIndex.Add(opHandle);
        }
    }

    public void LoadPlayerCharacters(List<CharacterObject> characters)
    {
        float characterCount = characters.Count();
        float padding = 0;
        if (characterCount % 2 == 0)
        {
            padding = 0.5f / characterCount;
        }
        else if (characterCount == 1) padding = 0.5f;

        for (int i = 0; i < characters.Count; i++)
        {
            var spawnPos = GetPlayerSpawns(padding + (float)i / (float)characters.Count);

            SpawnCharacterWithRarity(i, characters[i], spawnPos, gachaSystem.RandomRarity);
        }
    }

    IEnumerator LoadCharacter(int index, Vector3 spawnPos, AsyncOperationHandle<CharacterObject> obj)
    {
        yield return obj;

        var mState = PlayerData.MaggotStates[PlayerData.Party[index]];
        var characterObject = obj.Result;
        var pState = BattleData.PlayerStates.Count > 0 ? BattleData.PlayerStates[index] : null;
        var level = characterObject.GetLevelFromExp(mState.Exp);

        characterLoader.SpawnCharacterWithRarity(index, characterObject, spawnPos, Rarity.Common, level, pState);
        playersLoaded++;
    }

    public void SpawnCharacterWithRarity(int index, CharacterObject character, Vector3 spawnPos, Rarity rarity, int level = 1,
    BattleState.PlayerState stateInfo = null)
    {
        int playerCount = battleSystem.PlayerCharacters.Count(item => item != null);

        PlayerCharacter player = Instantiate(playerPrefab, spawnPos, Quaternion.identity).GetComponent<PlayerCharacter>();
        player.SetCharacterAndRarity(character, rarity);
        player.InitializeWithInfo(level, stateInfo);
        battleSystem.PlayerCharacters[index] = player;
    }

    public void LoadAllEnemies()
    {
        enemiesLoaded = 0;

        var currentWave = BattleData.EnemyGUIDs[BattleData.WaveCount];

        float enemyCount = currentWave.Count(e => e != null);
        float padding = 0;
        if (enemyCount % 2 == 0)
        {
            padding = 0.5f / enemyCount;
        }
        else if (enemyCount == 1) padding = 0.5f;

        for (int i = 0; i < currentWave.Count; i++)
        {
            string guid = currentWave[i];

            if (guid.IsNullEmptyOrWhiteSpace()) continue;
            var opHandle = Addressables.LoadAssetAsync<CharacterObject>(currentWave[i]);
            var spawnPos = GetEnemySpawns(padding + (float)i / enemyCount);
            StartCoroutine(LoadEnemy(i, spawnPos, opHandle));
            enemyHandleToIndex.Add(opHandle);
        }
    }

    public void LoadAllEnemies(WaveObject wave)
    {
        float enemyCount = wave.Enemies.Count(e => e != null);
        float padding = 0;
        if (enemyCount % 2 == 0)
        {
            padding = 0.5f / enemyCount;
        }
        else if (enemyCount == 1) padding = 0.5f;

        for (int i = 0; i < wave.Enemies.Length; i++)
        {
            if (wave.Enemies[i] == null) continue;

            var opHandle = Addressables.LoadAssetAsync<CharacterObject>(wave.Enemies[i]);
            var spawnPos = GetEnemySpawns(padding + (float)i / enemyCount);
            StartCoroutine(LoadEnemy(i, spawnPos, opHandle));
            enemyHandleToIndex.Add(opHandle);
        }
    }

    IEnumerator LoadEnemy(int index, Vector3 spawnPos, AsyncOperationHandle<CharacterObject> obj)
    {
        yield return obj;

        switch (index)
        {
            case 1:
                if (obj.Result.isBoss)
                {
                    var s = BattleData.EnemyStates.Count > 0 ? BattleData.EnemyStates[index] : null;
                    enemies[index] = SpawnBoss(obj.Result, spawnPos, BattleData.RoomLevel, s);
                }
                break;
        }

        BattleState.EnemyState state = BattleData.EnemyStates.Count > 0 ? BattleData.EnemyStates[index] : null;
        enemies[index] = SpawnEnemy(obj.Result, spawnPos, BattleData.RoomLevel, state);
        enemiesLoaded++;

        if (EnemiesLoaded)
        {
            enemyController.AssignEnemies(enemies);
            if (enemyHandleToIndex.Any(c => c.Result.isBoss)) EnemyWaveManager.OnEnterBossWave?.Invoke();
        }
    }

    public EnemyCharacter SpawnEnemy(CharacterObject character, Vector3 spawnPos, int level = 1, BattleState.EnemyState stateInfo = null)
    {
        EnemyCharacter newEnemy = Instantiate(enemyPrefab.gameObject, spawnPos, Quaternion.identity).GetComponent<EnemyCharacter>();
        newEnemy.SetCharacterAndRarity(character, Rarity.Common);
        newEnemy.InitializeWithInfo(level, stateInfo);
        //if (gachaSystem.LegacyMode)
        //{
        //    newEnemy.EnableBillboardUI();
        //}

        return newEnemy;
    }

    public EnemyCharacter SpawnBoss(CharacterObject character, Vector3 spawnPos, int level = 1, BattleState.EnemyState stateInfo = null)
    {
        EnemyCharacter newEnemy = Instantiate(bossPrefab.gameObject, spawnPos, Quaternion.identity).GetComponent<EnemyCharacter>();
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