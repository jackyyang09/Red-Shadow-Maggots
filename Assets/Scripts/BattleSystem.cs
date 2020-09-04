using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlePhases
{
    Entry,
    PlayerTurn,
    EnemyTurn,
    BattleWin,
    BattleLose
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField]
    BattlePhases currentPhase;

    [SerializeField]
    PlayerCharacter[] playerCharacters;

    [SerializeField]
    PlayerCharacter activePlayer;

    List<EnemyCharacter> enemies;

    [SerializeField]
    EnemyCharacter enemyTarget;

    public static BattleSystem instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerCharacters = FindObjectsOfType<PlayerCharacter>();
        activePlayer = playerCharacters[0];

        enemies = EnemyWaveManager.instance.SetupNextWave();
        enemyTarget = enemies[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExecutePlayerAttack()
    {
        activePlayer.PlayAttackAnimation();
        SceneTweener.instance.MeleeTweenTo(activePlayer.transform, enemyTarget.transform);
    }

    public void PlayerSelectCharacter(PlayerCharacter player)
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                activePlayer = player;
                break;
        }
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
