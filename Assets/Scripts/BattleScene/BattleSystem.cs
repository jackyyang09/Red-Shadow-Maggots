using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Facade;
using DocumentFormat.OpenXml.Office2016.Presentation.Command;

public enum BattlePhases
{
    Entry,
    PlayerTurn,
    EnemyTurn,
    BattleWin,
    BattleLose,
    Count
}

public static class BattlePhaseExtensions
{
    public static int ToInt(this BattlePhases bp) => (int)bp;
}

/// <summary>
/// If its the player's turn, player refers to the attacker and enemy refers to the defender
/// <para>Vice-versa if its the enemy's turn</para>
/// </summary>
public struct TargetedCharacters
{
    public PlayerCharacter player;
    public EnemyCharacter enemy;
}

public class BattleSystem : BasicSingleton<BattleSystem>
{
    public static float QuickTimeCritModifier = 0.15f;

    [SerializeField] BattlePhases currentPhase;
    public BattlePhases CurrentPhase => currentPhase;

    [SerializeField] private SkillManagerUI _skillManagerUI;

    bool finishedTurn;

    public bool FinishedTurn => finishedTurn;
    public void FinishTurn() => finishedTurn = true;

    PlayerCharacter[] playerCharacters = new PlayerCharacter[4];

    public PlayerCharacter[] PlayerCharacters => playerCharacters;

    public PlayerCharacter RandomPlayerCharacter
    {
        get
        {
            if (priorityPlayers.Count > 0) return priorityPlayers[0];

            List<PlayerCharacter> p = new List<PlayerCharacter>();
            for (int i = 0; i < playerCharacters.Length; i++)
            {
                if (PlayerCharacters[i])
                {
                    if (!PlayerCharacters[i].IsDead) p.Add(PlayerCharacters[i]);
                }
            }

            if (p.Count == 0) return null;
            battleStateManager.InitializeRandom();
            return p[Random.Range(0, p.Count)];
        }
    }

    public bool PlayersAlive
    {
        get
        {
            for (int i = 0; i < playerCharacters.Length; i++)
            {
                if (playerCharacters[i])
                {
                    if (!playerCharacters[i].IsDead) return true;
                }
            }

            return false;
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

    public PlayerCharacter EnemyAttackTarget
    {
        get { return enemyTargets.player; }
    }

    public EnemyCharacter EnemyAttacker
    {
        get { return enemyTargets.enemy; }
    }

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

    [SerializeField] TargetedCharacters playerTargets = new TargetedCharacters();

    [SerializeField] TargetedCharacters enemyTargets = new TargetedCharacters();

    List<BaseCharacter> moveOrder = new List<BaseCharacter>();
    int moveCount = 0;
    public int MoveCount => moveCount;
    public void SetMoveCount(int newCount) => moveCount = newCount;

    List<PlayerCharacter> priorityPlayers = new List<PlayerCharacter>();
    List<EnemyCharacter> priorityEnemies = new List<EnemyCharacter>();

    List<PlayerCharacter> deadMaggots = new List<PlayerCharacter>();
    List<AppliedEffect> deathEffects = new List<AppliedEffect>();
    public void RegisterOnDeathEffect(AppliedEffect a) => deathEffects.Add(a);

    public static System.Action[] OnStartPhase = new System.Action[(int)BattlePhases.Count];
    public static System.Action[] OnStartPhaseLate = new System.Action[(int)BattlePhases.Count];
    public static System.Action[] OnEndPhase = new System.Action[(int)BattlePhases.Count];
    public static System.Action OnEndTurn;

    public static System.Action OnTargetableCharactersChanged;
    public static System.Action<BaseGameEffect> OnTickEffect;
    public static System.Action OnFinishTickingEffects;

    public static System.Action OnWaveClear;
    public static System.Action OnFinalWaveClear;

    public static bool Initialized;

    private void OnEnable()
    {
        GlobalEvents.OnAnyPlayerDeath += OnAnyPlayerDeath;
        GlobalEvents.OnAnyEnemyDeath += SwitchTargets;

        SceneTweener.OnBattleEntered += ChangeBattlePhase;
        SkillManagerUI.OnSkillActivated += ActivateSkill;

        BaseCharacter.OnCharacterDeath += OnCharacterDeath;
    }

    private void OnDisable()
    {
        GlobalEvents.OnAnyPlayerDeath -= OnAnyPlayerDeath;
        GlobalEvents.OnAnyEnemyDeath -= SwitchTargets;

        SceneTweener.OnBattleEntered -= ChangeBattlePhase;
        SkillManagerUI.OnSkillActivated -= ActivateSkill;

        BaseCharacter.OnCharacterDeath -= OnCharacterDeath;
    }

    private IEnumerator Start()
    {
        screenEffects.BlackOut(ScreenEffects.EffectType.Fullscreen);

        yield return new WaitUntil(() => PlayerSaveManager.Initialized && BattleStateManager.Initialized);

        waveManager.SetupWave();

        if (gachaSystem.LegacyMode)
        {
            yield return new WaitUntil(() => playerCharacters.Any(t => t != null) && enemyController.Enemies != null);
            SetActiveEnemy(enemyController.RandomEnemy);
        }
        else
        {
            characterLoader.LoadAllPlayerCharacters();
            yield return new WaitUntil(() => characterLoader.PlayersLoaded && characterLoader.EnemiesLoaded);
            gameManager.LoadBattleState();
        }

        // Initialize turn order
        for (int i = 0; i < playerCharacters.Length; i++)
        {
            if (!playerCharacters[i]) continue;
            yield return new WaitUntil(() => playerCharacters[i].Initialized);
            if (playerCharacters[i].IsDead) continue;
            moveOrder.Add(playerCharacters[i]);
        }

        for (int i = 0; i < enemyController.Enemies.Length; i++)
        {
            if (!enemyController.Enemies[i]) continue;
            yield return new WaitUntil(() => enemyController.Enemies[i].Initialized);
            if (enemyController.Enemies[i].IsDead) continue;
            moveOrder.Add(enemyController.Enemies[i]);
        }

        currentPhase = BattlePhases.Entry;
        sceneTweener.EnterBattle();

        Initialized = true;
    }

    void OnDestroy()
    {
        Initialized = false;
    }

    public IEnumerator InitiateNextBattle()
    {
        waveManager.SetupWave();

        yield return characterLoader.EnemiesLoaded;

        for (int i = 0; i < deadMaggots.Count; i++)
        {
            Destroy(deadMaggots[i].gameObject);
        }

        sceneTweener.EnterBattle();
        currentPhase = BattlePhases.Entry;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Time.timeScale = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            Time.timeScale = 1;
        }
#endif
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

    public void OnAnyPlayerDeath()
    {
        SwitchTargets();
        DecrementMoveCount();
    }

    /// <summary>
    /// Auto-switch targets when current target is dead
    /// </summary>
    public void SwitchTargets()
    {
        if (playerTargets.enemy.IsDead)
        {
            var newTarget = enemyController.RandomEnemy;
            if (newTarget)
            {
                playerTargets.enemy = newTarget;
                playerTargets.enemy.ShowCharacterUI();
            }
        }
        else if (playerTargets.player.IsDead)
        {
            if (PlayersAlive)
            {
                playerTargets.player.ForceDeselect();
                for (int i = 0; i < playerCharacters.Length; i++)
                {
                    if (!playerCharacters[i].IsDead)
                    {
                        playerCharacters[i].ForceSelect();
                        playerTargets.player = playerCharacters[i];
                        break;
                    }
                }

                enemyTargets.player = playerTargets.player;
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
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (!enemies[i]) continue;
                    if (playerTargets.player.Reference.attackEffectPrefab != null)
                    {
                        enemies[i].SpawnEffectPrefab(playerTargets.player.Reference.attackEffectPrefab);
                    }

                    enemies[i].TakeDamage();
                }

                break;
            case BattlePhases.EnemyTurn:
                for (int i = 0; i < playerCharacters.Length; i++)
                {
                    if (!playerCharacters[i]) continue;
                    if (enemyTargets.enemy.Reference.attackEffectPrefab != null)
                    {
                        playerCharacters[i].SpawnEffectPrefab(enemyTargets.enemy.Reference.attackEffectPrefab);
                    }

                    playerCharacters[i].TakeDamage();
                }

                break;
        }
    }

    public void ActivateSkill(GameSkill skill)
    {
        if (playerTargets.player.CanUseSkill(skill))
        {
            StartCoroutine(SkillUseSequence(skill));
        }
    }

    IEnumerator SkillUseSequence(GameSkill skill)
    {
        bool finished = false;
        float skillUseTime = 1.5f;

        // Listen for if Player activates skills
        playerTargets.player.RegisterOnSkillFoundTargets(() => finished = true);

        // Activate Skill
        playerTargets.player.UseSkill(skill);

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

        gameManager.SaveBattleState();

        ui.ShowBattleUI();
    }

    public void TrySetActivePlayer(PlayerCharacter player)
    {
        if (battleSystem.ActivePlayer == player) return;
        if (player.IsDead) return;
        SetActivePlayer(player);
    }

    public void SetActivePlayer(PlayerCharacter player)
    {
        switch (currentPhase)
        {
            case BattlePhases.PlayerTurn:
                playerTargets.player = player;
                BaseCharacter.OnSelectCharacter?.Invoke(player);
                PlayerCharacter.OnSelectedPlayerCharacterChange?.Invoke(player);
                break;
            case BattlePhases.EnemyTurn:
                enemyTargets.player = player;
                break;
        }
    }

    public void TrySetActiveEnemy(EnemyCharacter enemy)
    {
        if (!enemy) return;
        if (enemy.IsDead) return;
        if (battleSystem.ActiveEnemy == enemy) return;
        battleSystem.SetActiveEnemy(enemy);
    }

    public void SetActiveEnemy(EnemyCharacter enemy)
    {
        switch (currentPhase)
        {
            case BattlePhases.Entry:
            case BattlePhases.PlayerTurn:
                playerTargets.enemy = enemy;
                BaseCharacter.OnSelectCharacter?.Invoke(enemy);
                EnemyCharacter.OnSelectedEnemyCharacterChange?.Invoke(enemy);
                break;
            case BattlePhases.EnemyTurn:
                enemyTargets.enemy = enemy;
                break;
        }
    }

    public void SetEnemyAttackTarget(PlayerCharacter player) => enemyTargets.player = player;
    public void SetEnemyAttacker(EnemyCharacter enemy) => enemyTargets.enemy = enemy;

    public void ChangeBattlePhase()
    {
        StartCoroutine(ChangePhaseRoutine());
    }

    public void EndTurn()
    {
        IncrementMoveCount();
        ChangeBattlePhase();
    }

    public void IncrementMoveCount() => moveCount = (int)Mathf.Repeat(moveCount + 1, moveOrder.Count);
    public void DecrementMoveCount() => moveCount = (int)Mathf.Repeat(moveCount - 1, moveOrder.Count);

    public IEnumerator ChangePhaseRoutine()
    {
        if (CurrentPhase == BattlePhases.EnemyTurn)
        {
            enemyTargets.enemy.IncreaseChargeLevel();
        }

        OnEndPhase[currentPhase.ToInt()]?.Invoke();
        foreach (var effect in deathEffects)
        {
            effect.OnDeath();

            yield return new WaitForSeconds(sceneTweener.EffectTickTime);

            // DIE
        }
        deathEffects.Clear();

        var activeCharacter = moveOrder[moveCount];
        var isPlayer = activeCharacter as PlayerCharacter;
        var lastPhase = currentPhase;
        if (isPlayer)
        {
            if (lastPhase == BattlePhases.EnemyTurn)
            {
                yield return StartCoroutine(TickEffects(new List<BaseCharacter>(enemyController.Enemies)));
                gameManager.SaveBattleState();
                OnEndTurn?.Invoke();
            }

            currentPhase = BattlePhases.PlayerTurn;

            if (!enemyController.EnemiesAlive)
            {
                currentPhase = BattlePhases.BattleWin;
            }
            SetActivePlayer(activeCharacter as PlayerCharacter);
        }
        else
        {
            if (lastPhase != BattlePhases.EnemyTurn)
            {
                yield return StartCoroutine(TickEffects(new List<BaseCharacter>(playerCharacters)));
                enemyController.CalculateSkillUsage();
            }

            if (PlayersAlive)
            {
                currentPhase = BattlePhases.EnemyTurn;
                SetActiveEnemy(activeCharacter as EnemyCharacter);
            }
            else if (!PlayersAlive) currentPhase = BattlePhases.BattleLose;
            else if (!enemyController.EnemiesAlive) currentPhase = BattlePhases.BattleWin;
        }

        finishedTurn = false;

        switch (currentPhase)
        {
            case BattlePhases.Entry:
                break;
            case BattlePhases.PlayerTurn:
                OnStartPhase[BattlePhases.PlayerTurn.ToInt()]?.Invoke();
                enemyController.ChooseAttackTarget();
                OnStartPhaseLate[BattlePhases.PlayerTurn.ToInt()]?.Invoke();
                break;
            case BattlePhases.EnemyTurn:
                OnStartPhase[BattlePhases.EnemyTurn.ToInt()]?.Invoke();
                enemyController.MakeYourMove();
                OnStartPhaseLate[BattlePhases.EnemyTurn.ToInt()]?.Invoke();
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
                OnStartPhase[BattlePhases.BattleLose.ToInt()]?.Invoke();
                break;
        }

        yield return null;
    }

    IEnumerator TickEffects(List<BaseCharacter> affectedCharacters)
    {
        Dictionary<BaseGameEffect, List<BaseCharacter>> effects = new Dictionary<BaseGameEffect, List<BaseCharacter>>();
        for (int i = 0; i < affectedCharacters.Count; i++)
        {
            if (!affectedCharacters[i]) continue;
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

        bool delay = false;
        var effectKeys = effects.GetKeysCached();
        for (int i = 0; i < effectKeys.Length; i++)
        {
            var characters = effects[effectKeys[i]];
            for (int j = 0; j < characters.Count; j++)
            {
                characters[j].TickEffect(effectKeys[i]);
            }

            OnTickEffect?.Invoke(effectKeys[i]);

            if (effectKeys[i].TickAnimationTime > -1)
            {
                if (effectKeys[i].TickAnimationTime > 0) delay = true;
                yield return new WaitForSeconds(effectKeys[i].TickAnimationTime);
            }
            else
            {
                delay = true;
                yield return new WaitForSeconds(sceneTweener.EffectTickTime);
            }
        }

        if (delay)
        {
            yield return new WaitForSeconds(sceneTweener.PostEffectTickTime);
        }

        OnFinishTickingEffects?.Invoke();
    }

    public void RegisterPlayerDeath(PlayerCharacter player)
    {
        deadMaggots.Add(player);
        player.transform.parent = null;
    }

    private void OnCharacterDeath(BaseCharacter obj)
    {
        moveOrder.Remove(obj);
        moveCount = (int)Mathf.Repeat(moveCount, moveOrder.Count);
    }

    public void ApplyTargetFocus(PlayerCharacter player)
    {
        priorityPlayers.Add(player);
        OnTargetableCharactersChanged?.Invoke();
    }

    public void RemoveTargetFocus(PlayerCharacter player)
    {
        priorityPlayers.Remove(player);
        OnTargetableCharactersChanged?.Invoke();
    }

    public void ApplyTargetFocus(EnemyCharacter enemy)
    {
        priorityEnemies.Add(enemy);
        OnTargetableCharactersChanged?.Invoke();
    }

    public void RemoveTargetFocus(EnemyCharacter enemy)
    {
        priorityEnemies.Remove(enemy);
        OnTargetableCharactersChanged?.Invoke();
    }

    #region Debug Hacks

    [IngameDebugConsole.ConsoleMethod(nameof(MaxPlayerCrit), "Set player characters crit chance to 100%")]
    public static void MaxPlayerCrit()
    {
        for (int i = 0; i < Instance.playerCharacters.Length; i++)
        {
            if (!Instance.playerCharacters[i]) continue;
            Instance.playerCharacters[i].ApplyCritChanceModifier(1);
        }

        Debug.Log("Crit rate maxed!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(AddPlayerCrit),
        "Set player characters crit chance to a number from 0 to 1")]
    public static void AddPlayerCrit(float value)
    {
        for (int i = 0; i < Instance.playerCharacters.Length; i++)
        {
            Instance.playerCharacters[i].ApplyCritChanceModifier(value);
        }

        Debug.Log("Added " + value + "% to player crit rate to!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(ShieldPlayers), "Provide Max Shield Effect to Players")]
    public static void ShieldPlayers()
    {
        for (int i = 0; i < Instance.playerCharacters.Length; i++)
        {
            Instance.playerCharacters[i].GiveShield(Instance.playerCharacters[i].MaxHealth);
        }

        Debug.Log("Players damaged!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(CripplePlayers), "Instantly hurt players, leaving them at 1 health")]
    public static void CripplePlayers()
    {
        for (int i = 0; i < Instance.PlayerCharacters.Length; i++)
        {
            if (!Instance.PlayerCharacters[i]) continue;
            BaseCharacter.IncomingDamage.TrueDamage = Instance.playerCharacters[i].CurrentHealth - 1;
            Instance.playerCharacters[i].TakeDamage();
        }

        Debug.Log("Players damaged!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(SetTimeScale), "Changes the standard game speed")]
    public void SetTimeScale(float value)
    {
        Time.timeScale = value;
    }

    #endregion
}