using System.Linq;
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

    List<PlayerCharacter> playerList;
    public List<PlayerCharacter> PlayerList
    {
        get
        {
            if (playerList == null)
            {
                playerList = playerCharacters.Where(e => e != null).ToList();
            }
            return playerList;
        }
    }

    public List<PlayerCharacter> LivingPlayers
    {
        get
        {
            return PlayerList.Where(e => !e.IsDead).ToList();
        }
    }

    public PlayerCharacter RandomLivingPlayer
    {
        get
        {
            if (priorityPlayers.Count > 0) return priorityPlayers[0];

            List<PlayerCharacter> p = LivingPlayers;

            battleStateManager.InitializeRandom();
            return p[Random.Range(0, p.Count)];
        }
    }

    public bool PlayersAlive => LivingPlayers.Count > 0;

    public List<BaseCharacter> AllCharacters
    {
        get
        {
            List<BaseCharacter> list = new List<BaseCharacter>(PlayerList);
            list.AddRange(enemyController.EnemyList);
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

    public EnemyCharacter PlayerAttackTarget => playerTargets.enemy;
    public PlayerCharacter EnemyAttackTarget => enemyTargets.player;
    public EnemyCharacter EnemyAttacker => enemyTargets.enemy;

    /// <summary>
    /// The attack target for the current turn
    /// </summary>
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
    public List<BaseCharacter> MoveOrder => moveOrder;

    List<PlayerCharacter> priorityPlayers = new List<PlayerCharacter>();
    List<EnemyCharacter> priorityEnemies = new List<EnemyCharacter>();

    List<PlayerCharacter> deadMaggots = new List<PlayerCharacter>();
    List<AppliedEffect> deathEffects = new List<AppliedEffect>();
    public void RegisterOnDeathEffect(AppliedEffect a) => deathEffects.Add(a);

    public static System.Action[] OnStartPhase = new System.Action[(int)BattlePhases.Count];
    public static System.Action[] OnStartPhaseLate = new System.Action[(int)BattlePhases.Count];
    public static System.Action[] OnEndPhase = new System.Action[(int)BattlePhases.Count];
    public static System.Action OnEndTurn;
    public static System.Action OnEndRound;

    public static System.Action OnMoveOrderUpdated;

    public static System.Action OnTargetableCharactersChanged;
    public static System.Action OnFinishTickingEffects;

    public static System.Action OnWaveClear;
    public static System.Action OnFinalWaveClear;

    public static bool Initialized;

    private void OnEnable()
    {
        GlobalEvents.OnAnyPlayerDeath += OnAnyPlayerDeath;
        GlobalEvents.OnAnyEnemyDeath += SwitchTargets;

        SkillManagerUI.OnSkillActivated += ActivateSkill;

        BaseCharacter.OnCharacterDeath += OnCharacterDeath;
    }

    private void OnDisable()
    {
        GlobalEvents.OnAnyPlayerDeath -= OnAnyPlayerDeath;
        GlobalEvents.OnAnyEnemyDeath -= SwitchTargets;

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
            currentPhase = BattlePhases.PlayerTurn;
        }

        // Initialize move order
        foreach (var player in PlayerList)
        {
            yield return new WaitUntil(() => player.Initialized);
            if (player.IsDead) continue;
            moveOrder.Add(player);
        }

        foreach (var enemy in enemyController.EnemyList)
        {
            yield return new WaitUntil(() => enemy.Initialized);
            if (enemy.IsDead) continue;
            moveOrder.Add(enemy);
        }

        UpdateMoveOrder(moveOrder.Any(e => e.WaitTimer == 0));

        if (moveOrder[0].IsPlayer())
        {
            currentPhase = BattlePhases.PlayerTurn;
            playerTargets.player = moveOrder[0] as PlayerCharacter;
            playerTargets.player.ShowSelectionCircle();
            playerTargets.enemy.ShowSelectionCircle();
        }
        else
        {
            currentPhase = BattlePhases.Entry;
            ChangeBattlePhase();
        }
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
        if (PlayersAlive) enemyController.ChooseAttackTarget();
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
                playerTargets.player.HideSelectionPointer();

                var p = RandomLivingPlayer;

                p.ShowSelectionCircle();
                playerTargets.player = p;

                enemyTargets.player = playerTargets.player;
            }
        }
    }

    public void PerformAttack(BaseCharacter attacker, BaseCharacter defender)
    {
        if (attacker.Reference.attackEffectPrefab)
        {
            defender.SpawnEffectPrefab(attacker.Reference.attackEffectPrefab);
        }

        defender.TakeDamage();
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
        OnEndTurn?.Invoke();
        moveOrder[0].OnEndTurn?.Invoke();
        OnEndPhase[currentPhase.ToInt()]?.Invoke();
        moveOrder[0].IncrementWaitTimer();
        UpdateMoveOrder();

        ChangeBattlePhase();
    }

    public void UpdateMoveOrder(bool firstTime = false)
    {
        if (!moveOrder.Any(c => !c.IsOverWait) || firstTime) // If everyone is OverWait or battle just began
        {
            foreach (var character in moveOrder)
            {
                character.ResetWait();
                character.IncrementWaitTimer();
            }
            OnEndRound?.Invoke();
        }

        moveOrder = moveOrder.OrderBy(c => c.WaitPercentage).ThenByDescending(c => c.IsPlayer()).ToList();
        OnMoveOrderUpdated?.Invoke();
    }

    public IEnumerator ChangePhaseRoutine()
    {
        if (CurrentPhase == BattlePhases.EnemyTurn)
        {
            enemyTargets.enemy.IncreaseChargeLevel();
        }

        foreach (var effect in deathEffects)
        {
            effect.OnDeath();

            yield return new WaitForSeconds(sceneTweener.EffectTickTime);

            // DIE
        }
        deathEffects.Clear();

        var activeCharacter = moveOrder[0];
        var isPlayer = activeCharacter as PlayerCharacter;
        var lastPhase = currentPhase;

        yield return StartCoroutine(TickEffects(activeCharacter));

        if (isPlayer)
        {
            if (lastPhase == BattlePhases.EnemyTurn)
            {
                gameManager.SaveBattleState();
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

        activeCharacter.OnStartTurn?.Invoke();
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
                if (!OpposingCharacter) enemyController.ChooseAttackTarget();
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

    IEnumerator TickEffects(BaseCharacter affectedCharacters)
    {
        float longestTime = -1;
        foreach (var item in affectedCharacters.AppliedEffects)
        {
            if (item.referenceEffect.TickAnimationTime > longestTime)
            {
                longestTime = item.referenceEffect.TickAnimationTime;
            }
        }

        yield return affectedCharacters.TickEffects(sceneTweener.EffectTickTime);

        if (longestTime > sceneTweener.EffectTickTime)
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
}