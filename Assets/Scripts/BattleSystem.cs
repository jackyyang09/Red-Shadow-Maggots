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
    public static float QuickTimeCritModifier = 0.15f;

    [SerializeField]
    List<float> gameSpeeds = new List<float>();
    public int CurrentGameSpeed = 0;
    public float CurrentGameSpeedTime
    {
        get
        {
            return gameSpeeds[CurrentGameSpeed];
        }
    }

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
            if (priorityPlayers.Count > 0) return priorityPlayers[0];
            return playerCharacters[Random.Range(0, PlayerCharacters.Count)];
        }
    }

    [SerializeField]
    TargettedCharacters playerTargets = new TargettedCharacters();

    [SerializeField]
    TargettedCharacters enemyTargets = new TargettedCharacters();

    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    Transform leftSpawnPos;

    [SerializeField]
    Transform middleSpawnPos;

    [SerializeField]
    Transform rightSpawnPos;

    List<PlayerCharacter> priorityPlayers = new List<PlayerCharacter>();
    List<EnemyCharacter> priorityEnemies = new List<EnemyCharacter>();
    
    List<PlayerCharacter> deadMaggots = new List<PlayerCharacter>();

    public static System.Action onStartPlayerTurn;
    /// <summary>
    /// Invokes after onStartPlayerTurn
    /// </summary>
    public static System.Action onStartPlayerTurnLate;
    public static System.Action onStartEnemyTurn;

    public static BattleSystem instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    public void GameStart()
    {
        playerTargets.player = playerCharacters[0];
        playerTargets.player.ShowCharacterUI();

        InitiateNextBattle();
    }

    private void OnEnable()
    {
        GlobalEvents.onAnyPlayerDeath += SwitchTargets;
        GlobalEvents.onAnyEnemyDeath += SwitchTargets;
    }

    private void OnDisable()
    {
        GlobalEvents.onAnyPlayerDeath -= SwitchTargets;
        GlobalEvents.onAnyEnemyDeath -= SwitchTargets;
    }

    public void InitiateNextBattle()
    {
        var enemies = EnemyWaveManager.instance.SetupNextWave();
        EnemyController.instance.AssignEnemies(enemies);
        playerTargets.enemy = enemies[0];
        playerTargets.enemy.ShowCharacterUI();

        for (int i = 0; i < deadMaggots.Count; i++)
        {
            Destroy(deadMaggots[i].gameObject);
        }

        SceneTweener.instance.EnterBattle();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Time.timeScale = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            Time.timeScale = 1;
        }
    }

    public void SpawnCharacterWithRarity(CharacterObject character, Rarity rarity)
    {
        Transform spawnPos = null;
        switch (playerCharacters.Count)
        {
            case 0:
                spawnPos = leftSpawnPos;
                break;
            case 1:
                spawnPos = middleSpawnPos;
                break;
            case 2:
                spawnPos = rightSpawnPos;
                break;
        }
        PlayerCharacter player = Instantiate(playerPrefab, spawnPos).GetComponent<PlayerCharacter>();
        player.SetCharacterAndRarity(character, rarity);
        playerCharacters.Add(player);
    }

    public void ExecutePlayerAttack()
    {
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
                    playerTargets.enemy.ShowCharacterUI();
                }
                break;
            case BattlePhases.EnemyTurn:
                if (playerCharacters.Count > 0)
                {
                    for (int i = 0; i < playerCharacters.Count; i++)
                    {
                        playerCharacters[i].ForceDeselct();
                    }
                    playerTargets.player = enemyTargets.player;
                    enemyTargets.player.ForceSelect();
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
        if (playerTargets.player.CanUseSkill(index))
        {
            StartCoroutine(SkillUseSequence(index));
        }
    }



    IEnumerator SkillUseSequence(int index)
    {
        bool finished = false;
        float skillUseTime = 1.5f;

        // Listen for if Player activates skills
        playerTargets.player.RegisterOnSkillFoundTargets(() => finished = true);

        // Activate Skill
        playerTargets.player.UseSkill(index);

        // Wait for player to activate skills
        while (!finished) yield return null;

        UIManager.instance.RemovePlayerControl();

        SceneTweener.instance.SkillTween(playerTargets.player.transform, skillUseTime);

        yield return new WaitForSeconds(skillUseTime);

        SceneTweener.instance.SkillUntween();

        finished = false;

        playerTargets.player.RegisterOnFinishApplyingSkillEffects(() => finished = true);

        playerTargets.player.ResolveSkill();

        while (!finished) yield return null;

        UIManager.instance.ResumePlayerControl();
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
                break;
            case BattlePhases.PlayerTurn:
                onStartPlayerTurn?.Invoke();
                onStartPlayerTurnLate?.Invoke();
                break;
            case BattlePhases.EnemyTurn:
                onStartEnemyTurn?.Invoke();
                EnemyController.instance.MakeYourMove();
                break;
            case BattlePhases.BattleWin:
                if (EnemyWaveManager.instance.IsLastWave)
                {
                    GlobalEvents.onFinalWaveClear?.Invoke();
                }
                else
                {
                    SceneTweener.instance.WaveClearSequence();
                    GlobalEvents.onWaveClear?.Invoke();
                }
                break;
            case BattlePhases.BattleLose:
                GlobalEvents.onPlayerDefeat?.Invoke();
                break;
        }
    }

    public void RegisterPlayerDeath(PlayerCharacter player)
    {
        deadMaggots.Add(player);
        player.transform.parent = null;
        playerCharacters.Remove(player);
    }

    public void ApplyTargetFocus(PlayerCharacter player)
    {
        priorityPlayers.Add(player);
    }

    public void RemoveTargetFocus(PlayerCharacter player)
    {
        priorityPlayers.Remove(player);
    }

    public void ApplyTargetFocus(EnemyCharacter enemy)
    {
        priorityEnemies.Add(enemy);
    }

    public void RemoveTargetFocus(EnemyCharacter enemy)
    {
        priorityEnemies.Remove(enemy);
    }

    public void ToggleGameSpeed()
    {
        CurrentGameSpeed = (int)Mathf.Repeat(CurrentGameSpeed + 1, gameSpeeds.Count);
        Time.timeScale = gameSpeeds[CurrentGameSpeed];
        GlobalEvents.onModifyGameSpeed?.Invoke();
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

    public BaseCharacter GetOpposingCharacter()
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                return playerTargets.enemy;
        }
        return enemyTargets.player;
    }

    #region Debug Hacks
    public void HackMaxCritRate()
    {
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            playerCharacters[i].ApplyCritChanceModifier(1);
        }
    }
    #endregion

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
