using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public enum BattlePhases
{
    Entry,
    PlayerTurn,
    EnemyTurn,
    BattleWin,
    BattleLose
}

/// <summary>
/// If its the player's turn, player refers to the attacker and enemy refers to the defender
/// <para>Vice-versa if its the enemy's turn</para>
/// </summary>
public struct TargettedCharacters
{
    public PlayerCharacter player;
    public EnemyCharacter enemy;
}

public class BattleSystem : BasicSingleton<BattleSystem>
{
    [SerializeField] float effectTickTime = 2;

    public static float QuickTimeCritModifier = 0.15f;

    [SerializeField] List<float> gameSpeeds = new List<float>();

    public int CurrentGameSpeed = 0;
    public float CurrentGameSpeedTime
    {
        get
        {
            return gameSpeeds[CurrentGameSpeed];
        }
    }

    [SerializeField] BattlePhases currentPhase;
    public BattlePhases CurrentPhase { get { return currentPhase; } }
    bool finishedTurn;
    public bool FinishedTurn { get { return finishedTurn; } }
    public void FinishTurn() => finishedTurn = true;

    [SerializeField] List<PlayerCharacter> playerCharacters = null;

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
            return playerCharacters[UnityEngine.Random.Range(0, PlayerCharacters.Count)];
        }
    }

    public List<BaseCharacter> AllCharacters
    { 
        get
        {
            List<BaseCharacter> list = new List<BaseCharacter>(playerCharacters);
            list.AddRange(enemyController.Enemies);
            return list;
        }
    }

    [SerializeField] TargettedCharacters playerTargets = new TargettedCharacters();

    [SerializeField] TargettedCharacters enemyTargets = new TargettedCharacters();

    [SerializeField] GameObject playerPrefab = null;
    [SerializeField] GameObject player3Dprefab = null;

    [SerializeField] Transform leftSpawnPos = null;

    [SerializeField] Transform middleSpawnPos = null;

    [SerializeField] Transform rightSpawnPos = null;

    List<PlayerCharacter> priorityPlayers = new List<PlayerCharacter>();
    List<EnemyCharacter> priorityEnemies = new List<EnemyCharacter>();
    
    List<PlayerCharacter> deadMaggots = new List<PlayerCharacter>();

    public static Action OnStartPlayerTurn;
    /// <summary>
    /// Invokes after OnStartPlayerTurn
    /// </summary>
    public static Action OnStartPlayerTurnLate;
    public static Action OnEndPlayerTurn;

    public static Action OnStartEnemyTurn;
    /// <summary>
    /// Invokes after OnStartEnemyTurn
    /// </summary>
    public static Action OnStartEnemyTurnLate;
    public static Action OnEndEnemyTurn;

    public static Action OnTargettableCharactersChanged;
    public static Action<BaseGameEffect> OnTickEffect;

    public static Action OnEnterFinalWave;
    public static Action OnWaveClear;
    public static Action OnFinalWaveClear;
    public static Action OnPlayerDefeat;

    public void GameStart()
    {
        playerTargets.player = playerCharacters[0];
        playerTargets.player.ShowCharacterUI();

        InitiateNextBattle();
    }

    private void OnEnable()
    {
        GlobalEvents.OnAnyPlayerDeath += SwitchTargets;
        GlobalEvents.OnAnyEnemyDeath += SwitchTargets;
    }

    private void OnDisable()
    {
        GlobalEvents.OnAnyPlayerDeath -= SwitchTargets;
        GlobalEvents.OnAnyEnemyDeath -= SwitchTargets;
    }

    public void InitiateNextBattle()
    {
        var enemies = waveManager.SetupWave();

        if (waveManager.IsLastWave) OnEnterFinalWave?.Invoke();

        enemyController.AssignEnemies(enemies);
        playerTargets.enemy = enemies[0];
        playerTargets.enemy.ShowCharacterUI();

        for (int i = 0; i < deadMaggots.Count; i++)
        {
            Destroy(deadMaggots[i].gameObject);
        }

        sceneTweener.EnterBattle();
        StartCoroutine(ChangeBattlePhase());
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

        if (character.characterRig)
        {
            PlayerCharacter player = Instantiate(player3Dprefab, spawnPos).GetComponent<PlayerCharacter>();
            player.SetCharacterAndRarity(character, rarity);
            playerCharacters.Add(player);
        }
        else // Legacy
        {
            PlayerCharacter player = Instantiate(playerPrefab, spawnPos).GetComponent<PlayerCharacter>();
            player.SetCharacterAndRarity(character, rarity);
            playerCharacters.Add(player);
        }
    }

    public void BeginPlayerAttack()
    {
        playerTargets.player.BeginAttack(playerTargets.enemy.transform);
        //switch (playerTargets.player.Reference.range)
        //{
        //    case AttackRange.CloseRange:
        //        SceneTweener.Instance.MeleeTweenTo(playerTargets.player.transform, playerTargets.enemy.transform);
        //        break;
        //    case AttackRange.LongRange:
        //        SceneTweener.Instance.RangedTweenTo(playerTargets.player.CharacterMesh.transform, playerTargets.enemy.transform);
        //        break;
        //}
    }

    /// <summary>
    /// Auto-switch targets when current target is dead
    /// </summary>
    public void SwitchTargets()
    {
        if (playerTargets.enemy.IsDead)
        {
            if (enemyController.Enemies.Count > 0)
            {
                playerTargets.enemy = enemyController.RandomEnemy;
                playerTargets.enemy.ShowCharacterUI();
            }
        }
        else if (playerTargets.player.IsDead)
        {
            if (playerCharacters.Count > 0)
            {
                for (int i = 0; i < playerCharacters.Count; i++)
                {
                    playerCharacters[i].ForceDeselect();
                }
                playerTargets.player = enemyTargets.player;
                enemyTargets.player.ForceSelect();
            }
        }
    }

    public void AttackTarget()
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                if (playerTargets.player.Reference.attackEffectPrefab != null)
                {
                    var targetEnemy = playerTargets.enemy;
                    targetEnemy.SpawnEffectPrefab(playerTargets.player.Reference.attackEffectPrefab);
                }
                playerTargets.enemy.TakeDamage();
                break;
            case BattlePhases.EnemyTurn:
                if (enemyTargets.enemy.Reference.attackEffectPrefab != null)
                {
                    var targetPlayer = enemyTargets.player;
                    targetPlayer.SpawnEffectPrefab(enemyTargets.enemy.Reference.attackEffectPrefab);
                }
                enemyTargets.player.TakeDamage();
                break;
        }
    }

    public void AttackAOE()
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                var enemies = enemyController.Enemies;
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (playerTargets.player.Reference.attackEffectPrefab != null)
                    {
                        enemies[i].SpawnEffectPrefab(playerTargets.player.Reference.attackEffectPrefab);
                    }
                    enemies[i].TakeDamage();
                }
                break;
            case BattlePhases.EnemyTurn:
                for (int i = 0; i < playerCharacters.Count; i++)
                {
                    if (enemyTargets.enemy.Reference.attackEffectPrefab != null)
                    {
                        playerCharacters[i].SpawnEffectPrefab(enemyTargets.enemy.Reference.attackEffectPrefab);
                    }
                    playerCharacters[i].TakeDamage();
                }
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

        finished = false;

        playerTargets.player.AnimHelper.RegisterOnFinishSkillAnimation(() => finished = true);

        ui.HideBattleUI();

        SceneTweener.Instance.SkillTween(playerTargets.player.transform, skillUseTime);

        //yield return new WaitForSeconds(skillUseTime);
        while (!finished) yield return null;

        SceneTweener.Instance.SkillUntween();

        finished = false;

        playerTargets.player.RegisterOnFinishApplyingSkillEffects(() => finished = true);

        playerTargets.player.ResolveSkill();

        // Wait for skill effects to finish animating
        while (!finished) yield return null;

        ui.ShowBattleUI();
    }

    public void ActivateEnemySkill(EnemyCharacter enemy, int index)
    {
        StartCoroutine(EnemySkillSequence(enemy, index));
    }

    IEnumerator EnemySkillSequence(EnemyCharacter enemy, int index)
    {
        bool finished = false;
        float skillUseTime = 1.5f;

        // Activate Skill
        enemy.UseSkill(index);

        enemy.AnimHelper.RegisterOnFinishSkillAnimation(() => finished = true);

        SceneTweener.Instance.SkillTween(enemy.transform, skillUseTime);

        //yield return new WaitForSeconds(skillUseTime);
        while (!finished) yield return null;

        SceneTweener.Instance.SkillUntween();

        finished = false;

        enemy.RegisterOnFinishApplyingSkillEffects(() => finished = true);

        enemy.ResolveSkill();

        // Wait for skill effects to finish animating
        while (!finished) yield return null;

        EndTurn();
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

    public void SetEnemyAttackTarget(PlayerCharacter player) => enemyTargets.player = player;
    public void SetEnemyAttacker(EnemyCharacter enemy) => enemyTargets.enemy = enemy;

    public void EndTurn()
    {
        StartCoroutine(ChangeBattlePhase());
    }

    //bool enemyAttacked = false;
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

    public IEnumerator ChangeBattlePhase()
    {
        switch (currentPhase)
        {
            case BattlePhases.Entry:
                currentPhase = BattlePhases.PlayerTurn;
                break;
            case BattlePhases.PlayerTurn:
                yield return new WaitForSeconds(sceneTweener.EnemyTurnTransitionDelay);
                yield return StartCoroutine(TickEffects(new List<BaseCharacter>(playerCharacters)));
                if (enemyController.Enemies.Count > 0)
                {
                    currentPhase = BattlePhases.EnemyTurn;
                }
                else currentPhase = BattlePhases.BattleWin;
                OnEndPlayerTurn?.Invoke();
                break;
            case BattlePhases.EnemyTurn:
                yield return new WaitForSeconds(sceneTweener.PlayerTurnTransitionDelay);
                yield return StartCoroutine(TickEffects(new List<BaseCharacter>(enemyController.Enemies)));
                if (playerCharacters.Count > 0 && enemyController.Enemies.Count > 0)
                {
                    currentPhase = BattlePhases.PlayerTurn;
                }
                else if (playerCharacters.Count == 0) currentPhase = BattlePhases.BattleLose;
                else if (enemyController.Enemies.Count == 0) currentPhase = BattlePhases.BattleWin;
                enemyTargets.enemy.IncreaseChargeLevel();
                OnEndEnemyTurn?.Invoke();
                break;
            case BattlePhases.BattleWin:
                currentPhase = BattlePhases.PlayerTurn;
                break;
        }

        finishedTurn = false;

        switch (currentPhase)
        {
            case BattlePhases.Entry:
                break;
            case BattlePhases.PlayerTurn:
                OnStartPlayerTurn?.Invoke();
                enemyController.ChooseNewTargets();
                OnStartPlayerTurnLate?.Invoke();
                break;
            case BattlePhases.EnemyTurn:
                OnStartEnemyTurn?.Invoke();
                enemyController.MakeYourMove();
                OnStartEnemyTurnLate?.Invoke();
                break;
            case BattlePhases.BattleWin:
                if (waveManager.IsLastWave)
                {
                    OnFinalWaveClear?.Invoke();
                }
                else
                {
                    SceneTweener.Instance.WaveClearSequence();
                    waveManager.WaveCount++;
                    OnWaveClear?.Invoke();
                }
                break;
            case BattlePhases.BattleLose:
                OnPlayerDefeat?.Invoke();
                break;
        }

        yield return null;
    }

    IEnumerator TickEffects(List<BaseCharacter> affectedCharacters)
    {
        Dictionary<BaseGameEffect, List<BaseCharacter>> effects = new Dictionary<BaseGameEffect, List<BaseCharacter>>();
        for (int i = 0; i < affectedCharacters.Count; i++)
        {
            var characterFX = affectedCharacters[i].AppliedEffects;
            var keys = characterFX.GetKeysCached();
            for (int j = 0; j < keys.Length; j++)
            {
                if (!effects.ContainsKey(keys[j]))
                {
                    effects.Add(keys[j], new List<BaseCharacter>());
                }
                effects[keys[j]].Add(affectedCharacters[i]);
            }
        }

        var effectKeys = effects.GetKeysCached();
        for (int i = 0; i < effectKeys.Length; i++)
        {
            var characters = effects[effectKeys[i]];
            for (int j = 0; j < characters.Count; j++)
            {
                characters[j].TickEffect(effectKeys[i]);
            }

            OnTickEffect?.Invoke(effectKeys[i]);

            if (effectKeys[i].hasTickAnimation)
            {
                yield return new WaitForSeconds(effectTickTime);
            }
        }

        yield return null;
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
        OnTargettableCharactersChanged?.Invoke();
    }

    public void RemoveTargetFocus(PlayerCharacter player)
    {
        priorityPlayers.Remove(player);
        OnTargettableCharactersChanged?.Invoke();
    }

    public void ApplyTargetFocus(EnemyCharacter enemy)
    {
        priorityEnemies.Add(enemy);
        OnTargettableCharactersChanged?.Invoke();
    }

    public void RemoveTargetFocus(EnemyCharacter enemy)
    {
        priorityEnemies.Remove(enemy);
        OnTargettableCharactersChanged?.Invoke();
    }

    //public void ToggleGameSpeed()
    //{
    //    CurrentGameSpeed = (int)Mathf.Repeat(CurrentGameSpeed + 1, gameSpeeds.Count);
    //    Time.timeScale = gameSpeeds[CurrentGameSpeed];
    //}

    public PlayerCharacter ActivePlayer
    {
        get
        {
            switch (currentPhase)
            {
                case BattlePhases.PlayerTurn:
                    return playerTargets.player;
            }
            return enemyTargets.player;
        }
    }

    public EnemyCharacter ActiveEnemy
    {
        get
        {
            switch (currentPhase)
            {
                case BattlePhases.PlayerTurn:
                    return playerTargets.enemy;
            }
            return enemyTargets.enemy;
        }
    }

    public PlayerCharacter EnemyAttackTarget { get { return enemyTargets.player; } }
    public EnemyCharacter EnemyAttacker { get { return enemyTargets.enemy; } }

    public BaseCharacter OpposingCharacter
    {
        get
        {
            switch (currentPhase)
            {
                case BattlePhases.PlayerTurn:
                    return playerTargets.enemy;
            }
            return enemyTargets.player;
        }
    }

    #region Debug Hacks
    [CommandTerminal.RegisterCommand(Help = "Set player characters crit chance to 100%", MaxArgCount = 0)]
    public static void MaxPlayerCrit(CommandTerminal.CommandArg[] args)
    {
        for (int i = 0; i < Instance.playerCharacters.Count; i++)
        {
            Instance.playerCharacters[i].ApplyCritChanceModifier(1);
        }
        Debug.Log("Crit rate maxed!");
    }

    [CommandTerminal.RegisterCommand(Help = "Set player characters crit chance to 50%", MaxArgCount = 0)]
    public static void MinMaxPlayerCrit(CommandTerminal.CommandArg[] args)
    {
        for (int i = 0; i < Instance.playerCharacters.Count; i++)
        {
            Instance.playerCharacters[i].ApplyCritChanceModifier(0.5f);
        }
        Debug.Log("Crit rate min-maxed!");
    }

    [CommandTerminal.RegisterCommand(Help = "Set enemy characters crit chance to 100%", MaxArgCount = 0)]
    public static void MaxEnemyCrit(CommandTerminal.CommandArg[] args)
    {
        for (int i = 0; i < enemyController.Enemies.Count; i++)
        {
            enemyController.Enemies[i].ApplyCritChanceModifier(1);
        }
        Debug.Log("Enemy crit maxed!");
    }

    [CommandTerminal.RegisterCommand(Help = "Instantly hurt players, leaving them at 1 health", MaxArgCount = 0)]
    public static void CripplePlayers(CommandTerminal.CommandArg[] args)
    {
        for (int i = 0; i < Instance.playerCharacters.Count; i++)
        {
            BaseCharacter.IncomingDamage.damage = Instance.playerCharacters[i].CurrentHealth - 1;
            Instance.playerCharacters[i].TakeDamage();
        }
        Debug.Log("Players damaged!");
    }
    #endregion
}