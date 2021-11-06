using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    [SerializeField] int waveCount = 0;

    public bool IsLastWave { get { return waveCount == waves.Length - 1; } }

    public WaveObject CurrentWave { get { return waves[waveCount]; } }
    public int CurrentWaveCount { get { return waveCount; } }

    public int TotalWaves { get { return waves.Length; } }

    [SerializeField] WaveObject[] waves = null;

    [SerializeField] Transform leftSpawnPos = null;

    [SerializeField] Transform middleSpawnPos = null;

    [SerializeField] Transform rightSpawnPos = null;

    [SerializeField] GameObject enemyPrefab = null;

    [SerializeField] GameObject bossPrefab = null;

    public static EnemyWaveManager Instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    void Start()
    {
        //waveCount = -1;
    }

    public List<EnemyCharacter> SetupNextWave()
    {
        RemoveDeadEnemies();

        var enemies = new List<EnemyCharacter>();

        waveCount++;

        WaveObject newWave = waves[waveCount];

        if (newWave.leftEnemy)
        {
            enemies.Add(SpawnEnemy(newWave.leftEnemy, leftSpawnPos));
        }
        
        if (newWave.middleEnemy)
        {
            if (newWave.isBossWave)
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

        return enemies;
    }

    public void RemoveDeadEnemies()
    {
        if (leftSpawnPos.childCount > 0) Destroy(leftSpawnPos.GetChild(0).gameObject);
        if (middleSpawnPos.childCount > 0) Destroy(middleSpawnPos.GetChild(0).gameObject);
        if (rightSpawnPos.childCount > 0) Destroy(rightSpawnPos.GetChild(0).gameObject);
    }

    public EnemyCharacter SpawnEnemy(CharacterObject character, Transform spawnPos)
    {
        EnemyCharacter newEnemy = Instantiate(enemyPrefab.gameObject, spawnPos).GetComponent<EnemyCharacter>();
        newEnemy.SetCharacterAndRarity(character, Rarity.Common);

        return newEnemy;
    }

    public EnemyCharacter SpawnBoss(CharacterObject character, Transform spawnPos)
    {
        EnemyCharacter newEnemy = Instantiate(bossPrefab.gameObject, spawnPos).GetComponent<EnemyCharacter>();
        newEnemy.SetCharacterAndRarity(character, Rarity.Common);

        return newEnemy;
    }

    void EstablishSingletonDominance()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            // A unique case where the Singleton exists but not in this scene
            if (Instance.gameObject.scene.name == null)
            {
                Instance = this;
            }
            else if (!Instance.gameObject.activeInHierarchy)
            {
                Instance = this;
            }
            else if (Instance.gameObject.scene.name != gameObject.scene.name)
            {
                Instance = this;
            }
            Destroy(gameObject);
        }
    }
}
