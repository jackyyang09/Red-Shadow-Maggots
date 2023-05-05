using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Facade;

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
public struct TargettedCharacters
{
    public PlayerCharacter player;
    public EnemyCharacter enemy;
}

public class BattleSystem : BasicSingleton<BattleSystem>
{
    [SerializeField] float effectTickTime = 2;

    public static float QuickTimeCritModifier = 0.15f;

    [SerializeField] BattlePhases currentPhase;
    [SerializeField] private SkillManagerUI _skillManagerUI;

    public BattlePhases CurrentPhase
    {
        get { return currentPhase; }
    }

    bool finishedTurn;

    public bool FinishedTurn
    {
        get { return finishedTurn; }
    }

    public void FinishTurn() => finishedTurn = true;

    PlayerCharacter[] playerCharacters = new PlayerCharacter[3];

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

    [SerializeField] TargettedCharacters playerTargets = new TargettedCharacters();

    [SerializeField] TargettedCharacters enemyTargets = new TargettedCharacters();

    List<BaseCharacter> moveOrder = new List<BaseCharacter>();
    int moveCount = 0;

    List<PlayerCharacter> priorityPlayers = new List<PlayerCharacter>();
    List<EnemyCharacter> priorityEnemies = new List<EnemyCharacter>();

    List<PlayerCharacter> deadMaggots = new List<PlayerCharacter>();

    public static System.Action[] OnStartPhase = new System.Action[(int)BattlePhases.Count];
    public static System.Action[] OnStartPhaseLate = new System.Action[(int)BattlePhases.Count];
    public static System.Action[] OnEndPhase = new System.Action[(int)BattlePhases.Count];
    public static System.Action OnEndTurn;

    public static System.Action OnTargettableCharactersChanged;
    public static System.Action<BaseGameEffect> OnTickEffect;
    public static System.Action OnFinishTickingEffects;

    public static System.Action OnWaveClear;
    public static System.Action OnFinalWaveClear;

    private void OnEnable()
    {
        GlobalEvents.OnAnyPlayerDeath += SwitchTargets;
        GlobalEvents.OnAnyEnemyDeath += SwitchTargets;

        SceneTweener.OnBattleEntered += EndTurn;
        SkillManagerUI.OnSkillActivated += ActivateSkill;

        BaseCharacter.OnCharacterDeath += OnCharacterDeath;
    }

    private void OnDisable()
    {
        GlobalEvents.OnAnyPlayerDeath -= SwitchTargets;
        GlobalEvents.OnAnyEnemyDeath -= SwitchTargets;

        SceneTweener.OnBattleEntered -= EndTurn;
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
            yield return new WaitUntil(() => playerCharacters.Any(t => t != null) && enemyController.Enemies.Any(t => t != null));
            SetActiveEnemy(enemyController.RandomEnemy);
        }
        else
        {
            characterLoader.LoadAllPlayerCharacters();
            yield return new WaitUntil(() => characterLoader.PlayersLoaded && characterLoader.EnemiesLoaded);
            LoadBattleState();
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

        SaveBattleState();

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

    public void EndTurn()
    {
        StartCoroutine(ChangeBattlePhase());
    }

    public void IncrementMoveCount() => moveCount = (int)Mathf.Repeat(moveCount + 1, moveOrder.Count);

    public IEnumerator ChangeBattlePhase()
    {
        switch (currentPhase)
        {
            case BattlePhases.Entry:
                break;
            case BattlePhases.PlayerTurn:
                yield return new WaitForSeconds(sceneTweener.EnemyTurnTransitionDelay);
                break;
            case BattlePhases.EnemyTurn:
                yield return new WaitForSeconds(sceneTweener.PlayerTurnTransitionDelay);
                enemyTargets.enemy.IncreaseChargeLevel();
                break;
        }

        OnEndPhase[currentPhase.ToInt()]?.Invoke();

        var activeCharacter = moveOrder[moveCount];
        var isPlayer = activeCharacter as PlayerCharacter;
        var lastPhase = currentPhase;
        if (isPlayer)
        {
            if (lastPhase == BattlePhases.EnemyTurn)
            {
                yield return StartCoroutine(TickEffects(new List<BaseCharacter>(enemyController.Enemies)));
                SaveBattleState();
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
            if (lastPhase == BattlePhases.PlayerTurn)
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

        IncrementMoveCount();
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

        OnFinishTickingEffects?.Invoke();
    }

    void LoadBattleState()
    {
        gameManager.TurnCount = BattleData.TurnCount;
        moveCount = BattleData.MoveCount;
        canteenSystem.SetCanteenCharge(BattleData.StoredCharge);
        if (enemyController.Enemies[BattleData.SelectedEnemy].IsDead)
        {
            SetActiveEnemy(enemyController.RandomEnemy);
        }
        else
        {
            SetActiveEnemy(enemyController.Enemies[BattleData.SelectedEnemy]);
        }
    }

    void SaveBattleState()
    {
        if (gachaSystem.LegacyMode)
        {
            Debug.Log("Gacha System is in Legacy Mode, not saving");
            return;
        }

        var partyData = new List<BattleState.PlayerState>();
        var waveData = new List<BattleState.EnemyState>();

        var seed = (int)System.DateTime.Now.Ticks;
        BattleData.SavedSeed = seed;

        for (int i = 0; i < 3; i++)
        {
            BattleState.PlayerState p = null;
            var player = playerCharacters[i];
            if (player)
            {
                p = new BattleState.PlayerState();
                p.Health = player.CurrentHealth;
                p.Effects = new List<BattleState.SerializedEffect>();

                BaseGameEffect[] keys = new BaseGameEffect[player.AppliedEffects.Keys.Count];
                player.AppliedEffects.Keys.CopyTo(keys, 0);
                for (int j = 0; j < keys.Length; j++)
                {
                    for (int l = 0; l < player.AppliedEffects[keys[j]].Count; l++)
                    {
                        var se = gameEffectLoader.SerializeGameEffect(player.AppliedEffects[keys[j]][l]);
                        p.Effects.Add(se);
                    }
                }

                p.Cooldowns = new int[2];
                for (int j = 0; j < 2; j++)
                {
                    p.Cooldowns[j] = player.Skills[j].cooldownTimer;
                }
            }

            partyData.Add(p);

            BattleState.EnemyState d = null;
            var enemy = enemyController.Enemies[i];
            if (enemy)
            {
                d = new BattleState.EnemyState();
                d.Health = enemy.CurrentHealth;
                d.Crit = enemy.CritLevel;
                d.Effects = new List<BattleState.SerializedEffect>();

                BaseGameEffect[] keys = new BaseGameEffect[enemy.AppliedEffects.Keys.Count];
                enemy.AppliedEffects.Keys.CopyTo(keys, 0);
                for (int j = 0; j < keys.Length; j++)
                {
                    for (int l = 0; l < enemy.AppliedEffects[keys[j]].Count; l++)
                    {
                        var se = gameEffectLoader.SerializeGameEffect(enemy.AppliedEffects[keys[j]][l]);
                        d.Effects.Add(se);
                    }
                }
            }

            waveData.Add(d);
        }

        BattleData.PlayerStates = partyData;
        BattleData.EnemyStates = waveData;

        BattleData.StoredCharge = canteenSystem.AvailableCharge;
        BattleData.TurnCount = gameManager.TurnCount;
        BattleData.MoveCount = moveCount;
        BattleData.SelectedEnemy = new List<EnemyCharacter>(enemyController.Enemies).IndexOf(ActiveEnemy);

        battleStateManager.SaveData();
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

    [IngameDebugConsole.ConsoleMethod(nameof(CripplePlayers), "Instantly hurt players, leaving them at 1 health")]
    public static void CripplePlayers()
    {
        for (int i = 0; i < Instance.playerCharacters.Length; i++)
        {
            BaseCharacter.IncomingDamage.damage = Instance.playerCharacters[i].CurrentHealth - 1;
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