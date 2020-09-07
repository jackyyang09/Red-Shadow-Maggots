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

public struct TargettedCharacters
{
    public PlayerCharacter player;
    public EnemyCharacter enemy;
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField]
    BattlePhases currentPhase;
    public BattlePhases CurrentPhase
    {
        get
        {
            return currentPhase;
        }
    }

    [SerializeField]
    List<PlayerCharacter> playerCharacters;
    public List<PlayerCharacter> PlayerCharacters
    {
        get
        {
            return playerCharacters;
        }
    }
    public PlayerCharacter RandomPlayerCharacter
    {
        get
        {
            return playerCharacters[Random.Range(0, PlayerCharacters.Count)];
        }
    }

    [SerializeField]
    TargettedCharacters playerTargets = new TargettedCharacters();

    [SerializeField]
    TargettedCharacters enemyTargets = new TargettedCharacters();

    public static System.Action onStartPlayerTurn;
    public static System.Action onStartEnemyTurn;

    public static BattleSystem instance;

    private void Awake()
    {
        EstablishSingletonDominance();

        ScreenEffects.instance.FadeFromBlack();

        playerCharacters = new List<PlayerCharacter>(FindObjectsOfType<PlayerCharacter>());
    }

    // Start is called before the first frame update
    void Start()
    {
        playerTargets.player = playerCharacters[0];
        playerTargets.player.ShowCharacterUI();

        var enemies = EnemyWaveManager.instance.SetupNextWave();
        EnemyController.instance.AssignEnemies(enemies);
        playerTargets.enemy = enemies[0];
        playerTargets.enemy.ShowCharacterUI();

        GlobalEvents.onPlayerDeath += SwitchTargets;
        GlobalEvents.onEnemyDeath += SwitchTargets;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ExecutePlayerAttack()
    {
        GlobalEvents.onPlayerStartAttack?.Invoke(playerTargets.player);
        playerTargets.player.PlayAttackAnimation();
        SceneTweener.instance.MeleeTweenTo(playerTargets.player.transform, playerTargets.enemy.transform);
    }

    public void ExecuteEnemyAttack()
    {
        enemyTargets.enemy.PlayAttackAnimation();
        UIManager.instance.StartDefending();
        SceneTweener.instance.MeleeTweenTo(enemyTargets.enemy.transform, enemyTargets.player.transform);
    }

    public void SwitchTargets()
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                if (EnemyController.instance.enemies.Count > 0)
                {
                    playerTargets.enemy = EnemyController.instance.RandomEnemy;
                }
                break;
            case BattlePhases.EnemyTurn:
                if (playerCharacters.Count > 0)
                {
                    enemyTargets.player = RandomPlayerCharacter;
                }
                break;
        }
    }

    public void AttackTarget(DamageStruct damage)
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                playerTargets.enemy.TakeDamage(damage);
                break;
            case BattlePhases.EnemyTurn:
                enemyTargets.player.TakeDamage(damage);
                break;
        }
    }

    public void ActivateSkill(int index)
    {
        playerTargets.player.UseSkill(index);
    }

    public void SetActivePlayer(PlayerCharacter player)
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                playerTargets.player = player;
                break;
            case BattlePhases.EnemyTurn:
                enemyTargets.player = player;
                break;
        }
    }

    public void SetActiveEnemy(EnemyCharacter enemy)
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                playerTargets.enemy = enemy;
                break;
            case BattlePhases.EnemyTurn:
                enemyTargets.enemy = enemy;
                break;
        }
    }

    public void EndTurn()
    {
        StartCoroutine(TurnEndTransition());
    }

    IEnumerator TurnEndTransition()
    {
        yield return new WaitForSeconds(SceneTweener.instance.TurnTransitionDelay);

        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                if (EnemyController.instance.enemies.Count > 0)
                {
                    SetPhase(BattlePhases.EnemyTurn);
                }
                else SetPhase(BattlePhases.BattleWin);
                break;
            case BattlePhases.EnemyTurn:
                if (playerCharacters.Count > 0)
                {
                    SetPhase(BattlePhases.PlayerTurn);
                }
                else SetPhase(BattlePhases.BattleLose);
                break;
            case BattlePhases.BattleWin:
                break;
            case BattlePhases.BattleLose:
                break;
        }
    }

    bool enemyAttacked = false;
    //IEnumerator EnemyTurn()
    //{
    //    enemyAttacked = false;
    //    QuickTimeBase.onExecuteQuickTime += (x) => enemyAttacked = true;
    //    
    //    while (!enemyAttacked)
    //    {
    //        yield return null;
    //    }
    //    QuickTimeBase.onExecuteQuickTime -= (x) => enemyAttacked = true;
    //    
    //    yield return new WaitForSeconds(3);
    //    
    //    EndTurn();
    //}

    public void SetPhase(BattlePhases newPhase)
    {
        currentPhase = newPhase;
        switch (currentPhase)
        {
            case BattlePhases.Entry:
                onStartEnemyTurn?.Invoke();
                break;
            case BattlePhases.PlayerTurn:
                onStartPlayerTurn?.Invoke();
                break;
            case BattlePhases.EnemyTurn:
                EnemyController.instance.MakeYourMove();
                break;
            case BattlePhases.BattleWin:
                SceneTweener.instance.WaveClearSequence();
                GlobalEvents.onWaveClear?.Invoke();
                break;
            case BattlePhases.BattleLose:
                GlobalEvents.onPlayerDefeat?.Invoke();
                break;
        }
    }

    public void RegisterPlayerDeath(PlayerCharacter player)
    {
        playerCharacters.Remove(player);
    }

    public PlayerCharacter GetActivePlayer()
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                return playerTargets.player;
        }
        return enemyTargets.player;
    }

    public EnemyCharacter GetActiveEnemy()
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                return playerTargets.enemy;
        }
        return enemyTargets.enemy;
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
