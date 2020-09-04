using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    [SerializeField]
    int waveCount;

    [SerializeField]
    WaveObject[] waves;

    [SerializeField]
    Transform leftSpawnPos;

    [SerializeField]
    Transform middleSpawnPos;

    [SerializeField]
    Transform rightSpawnPos;

    [SerializeField]
    GameObject enemyPrefab;

    public static EnemyWaveManager instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    void Start()
    {
        waveCount = -1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<EnemyCharacter> SetupNextWave()
    {
        var enemies = new List<EnemyCharacter>();

        waveCount++;
        GlobalEvents.onEnterWave?.Invoke();

        WaveObject newWave = waves[waveCount];

        if (newWave.leftEnemy)
        {
            enemies.Add(SpawnEnemy(newWave.leftEnemy, leftSpawnPos));
        }
        
        if (newWave.middleEnemy)
        {
            enemies.Add(SpawnEnemy(newWave.middleEnemy, middleSpawnPos));
        }
        
        if (newWave.rightEnemy)
        {
            enemies.Add(SpawnEnemy(newWave.rightEnemy, rightSpawnPos));
        }

        return enemies;
    }

    public EnemyCharacter SpawnEnemy(CharacterObject character, Transform spawnPos)
    {
        EnemyCharacter newEnemy = Instantiate(enemyPrefab.gameObject, spawnPos).GetComponent<EnemyCharacter>();
        newEnemy.SetReference(character);
        newEnemy.ApplyReferenceProperties();

        return newEnemy;
    }

    void EstablishSingletonDominance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // A unique case where the Singleton exists but not in this scene
            if (instance.gameObject.scene.name == null)
            {
                instance = this;
            }
            else if (!instance.gameObject.activeInHierarchy)
            {
                instance = this;
            }
            else if (instance.gameObject.scene.name != gameObject.scene.name)
            {
                instance = this;
            }
            Destroy(gameObject);
        }
    }
}
