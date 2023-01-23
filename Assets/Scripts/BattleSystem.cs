using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

    [SerializeField] List<PlayerCharacter> playerCharacters = null;

    public List<PlayerCharacter> PlayerCharacters
    {
        get { return playerCharacters; }
    }

    public PlayerCharacter RandomPlayerCharacter
    {
        get
        {
            if (priorityPlayers.Count > 0) return priorityPlayers[0];

            List<PlayerCharacter> p = new List<PlayerCharacter>();
            for (int i = 0; i < playerCharacters.Count; i++)
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
            for (int i = 0; i < playerCharacters.Count; i++)
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

    [SerializeField] GameObject playerPrefab = null;
    [SerializeField] GameObject player3Dprefab = null;

    [SerializeField] Transform leftSpawnPos = null;

    [SerializeField] Transform middleSpawnPos = null;

    [SerializeField] Transform rightSpawnPos = null;

    List<PlayerCharacter> priorityPlayers = new List<PlayerCharacter>();
    List<EnemyCharacter> priorityEnemies = new List<EnemyCharacter>();

    List<PlayerCharacter> deadMaggots = new List<PlayerCharacter>();

    public static System.Action[] OnStartPhase = new System.Action[(int)BattlePhases.Count];
    public static System.Action[] OnStartPhaseLate = new System.Action[(int)BattlePhases.Count];
    public static System.Action[] OnEndPhase = new System.Action[(int)BattlePhases.Count];

    public static System.Action OnTargettableCharactersChanged;
    public static System.Action<BaseGameEffect> OnTickEffect;

    public static System.Action OnWaveClear;
    public static System.Action OnFinalWaveClear;

    private void OnEnable()
    {
        GlobalEvents.OnAnyPlayerDeath += SwitchTargets;
        GlobalEvents.OnAnyEnemyDeath += SwitchTargets;

        SceneTweener.OnBattleEntered += EndTurn;
        SkillManagerUI.OnSkillActivated += ActivateSkill;
    }

    private void OnDisable()
    {
        GlobalEvents.OnAnyPlayerDeath -= SwitchTargets;
        GlobalEvents.OnAnyEnemyDeath -= SwitchTargets;

        SceneTweener.OnBattleEntered -= EndTurn;
        SkillManagerUI.OnSkillActivated -= ActivateSkill;
    }

    private IEnumerator Start()
    {
        screenEffects.BlackOut();

        yield return new WaitUntil(() => PlayerDataManager.Initialized && BattleStateManager.Initialized);

        if (!gachaSystem.LegacyMode)
        {
            yield return StartCoroutine(LoadBattleState());
        }

        GameStart();
    }

    public void GameStart()
    {
        if (playerCharacters[0]) playerTargets.player = playerCharacters[0];
        else if (playerCharacters[1]) playerTargets.player = playerCharacters[1];
        else if (playerCharacters[2]) playerTargets.player = playerCharacters[2];
        playerTargets.player.ShowCharacterUI();

        StartCoroutine(InitiateNextBattle());
    }

    public IEnumerator InitiateNextBattle()
    {
        yield return waveManager.SetupWave();

        if (battleStateManager.LoadedData.SavedSeed == 0)
        {
            var seed = (int)System.DateTime.Now.Ticks;
            battleStateManager.LoadedData.SavedSeed = seed;
            battleStateManager.SaveData();
        }

        for (int i = 0; i < enemyController.Enemies.Length; i++)
        {
            if (!enemyController.Enemies[i]) continue;
            if (enemyController.Enemies[i].IsDead) continue;
            playerTargets.enemy = enemyController.Enemies[i];
            playerTargets.enemy.ShowCharacterUI();
            break;
        }

        enemyController.ChooseNewTargets();

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

    public void SpawnCharacterWithRarity(CharacterObject character, Rarity rarity, int level = 1,
        BattleState.PlayerState stateInfo = null)
    {
        Transform spawnPos = null;
        switch (playerCharacters.Count)
        {
            case 0:
                spawnPos = middleSpawnPos;
                break;
            case 1:
                spawnPos = leftSpawnPos;
                break;
            case 2:
                spawnPos = rightSpawnPos;
                break;
        }

        if (character.characterRig)
        {
            PlayerCharacter player = Instantiate(player3Dprefab, spawnPos).GetComponent<PlayerCharacter>();
            player.SetCharacterAndRarity(character, rarity);
            player.ApplyCharacterStats(level, stateInfo);
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
                for (int i = 0; i < playerCharacters.Count; i++)
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

    public void ActivateEnemySkill(EnemyCharacter enemy, GameSkill skill)
    {
        StartCoroutine(EnemySkillSequence(enemy, skill));
    }

    IEnumerator EnemySkillSequence(EnemyCharacter enemy, GameSkill skill)
    {
        bool finished = false;
        float skillUseTime = 1.5f;

        // Activate Skill
        enemy.UseSkill(skill);

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
                if (enemyController.EnemiesAlive)
                {
                    currentPhase = BattlePhases.EnemyTurn;
                }
                else currentPhase = BattlePhases.BattleWin;

                OnEndPhase[BattlePhases.PlayerTurn.ToInt()]?.Invoke();
                break;
            case BattlePhases.EnemyTurn:
                yield return new WaitForSeconds(sceneTweener.PlayerTurnTransitionDelay);
                yield return StartCoroutine(TickEffects(new List<BaseCharacter>(enemyController.Enemies)));
                if (PlayersAlive && enemyController.EnemiesAlive)
                {
                    currentPhase = BattlePhases.PlayerTurn;
                }
                else if (!PlayersAlive) currentPhase = BattlePhases.BattleLose;
                else if (!enemyController.EnemiesAlive) currentPhase = BattlePhases.BattleWin;

                enemyTargets.enemy.IncreaseChargeLevel();
                OnEndPhase[BattlePhases.EnemyTurn.ToInt()]?.Invoke();
                SaveBattleState();
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
                OnStartPhase[BattlePhases.PlayerTurn.ToInt()]?.Invoke();
                enemyController.ChooseNewTargets();
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

        yield return null;
    }

    IEnumerator LoadBattleState()
    {
        var battleData = battleStateManager.LoadedData;
        var playerData = playerDataManager.LoadedData;
        for (int i = 0; i < playerData.Party.Length; i++)
        {
            if (playerData.Party[i] == -1)
            {
                playerCharacters.Add(null);
                continue;
            }

            var mState = playerData.MaggotStates[playerData.Party[i]];
            var guid = mState.GUID;
            var opHandle = Addressables.LoadAssetAsync<CharacterObject>(guid);
            yield return opHandle;

            var characterObject = opHandle.Result;
            var pState = battleData.PlayerStates.Count > 0 ? battleData.PlayerStates[i] : null;
            var level = characterObject.GetLevelFromExp(mState.Exp);

            SpawnCharacterWithRarity(characterObject, Rarity.Common, level, pState);
        }

        canteenSystem.SetCanteenCharge(battleData.StoredCharge);
        gameManager.TurnCount = battleData.TurnCount;
    }

    void SaveBattleState()
    {
        if (gachaSystem.LegacyMode)
        {
            Debug.Log("Gacha System is in Legacy Mode, not saving");
            return;
        }

        var data = battleStateManager.LoadedData;

        var partyData = new List<BattleState.PlayerState>();
        var waveData = new List<BattleState.EnemyState>();

        var seed = (int)System.DateTime.Now.Ticks;
        data.SavedSeed = seed;

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

        data.PlayerStates = partyData;
        data.EnemyStates = waveData;

        data.StoredCharge = canteenSystem.AvailableCharge;
        data.TurnCount = gameManager.TurnCount;

        battleStateManager.SaveData();
    }

    public void RegisterPlayerDeath(PlayerCharacter player)
    {
        deadMaggots.Add(player);
        player.transform.parent = null;
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
        for (int i = 0; i < Instance.playerCharacters.Count; i++)
        {
            Instance.playerCharacters[i].ApplyCritChanceModifier(1);
        }

        Debug.Log("Crit rate maxed!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(AddPlayerCrit),
        "Set player characters crit chance to a number from 0 to 1")]
    public static void AddPlayerCrit(float value)
    {
        for (int i = 0; i < Instance.playerCharacters.Count; i++)
        {
            Instance.playerCharacters[i].ApplyCritChanceModifier(value);
        }

        Debug.Log("Added " + value + "% to player crit rate to!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(CripplePlayers), "Instantly hurt players, leaving them at 1 health")]
    public static void CripplePlayers()
    {
        for (int i = 0; i < Instance.playerCharacters.Count; i++)
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