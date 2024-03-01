using DG.Tweening;
using RSMConstants;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Facade;

public abstract class BaseCharacter : MonoBehaviour
{
    [SerializeField] protected CharacterObject characterReference;

    public CharacterObject Reference => characterReference;

    [SerializeField] [Range(1, 90)] protected int currentLevel = 1;
    public int CurrentLevel => currentLevel;

    [SerializeField] protected float health;
    public float CurrentHealth => health;

    [SerializeField] protected float maxHealth;
    public float MaxHealth => maxHealth;

    List<StatModifier> shieldMods = new List<StatModifier>();
    public float CurrentShield
    {
        get
        {
            var m = 0f;
            foreach (var item in shieldMods)
            {
                m += item.Value;
            }
            return m;
        }
    }
    public float ShieldPercent => CurrentShield / maxHealth;

    protected float damageAbsorptionModifier = 0;
    public float DamageAbsorptionModifier => damageAbsorptionModifier;

    float attackModifier;
    /// <summary>
    /// Additive modifier from skills
    /// </summary>
    public float AttackModifier => attackModifier;
    //public float AttackModifier
    //{
    //    get
    //    {
    //        var m = 0f;
    //        foreach (var item in attackMods)
    //        {
    //            item.
    //        }
    //        return m;
    //    }
    //}
    public float AttackModified => Attack + attackModifier;
    public float Attack => characterReference.GetAttack(currentLevel);
    //public List<BaseStatModifier> attackMods = new List<BaseStatModifier>();

    float defenseModifier;
    public float Defense => characterReference.GetDefense(currentLevel);
    /// <summary>
    /// Additive modifier from skills
    /// </summary>
    public float DefenseModifier => defenseModifier;
    public float DefenseModified => Defense + DefenseModifier;

    float attackLeniencyModifier;
    public float AttackLeniency => characterReference.attackLeniency;
    public float AttackLeniencyModifier => attackLeniencyModifier;
    public float AttackLeniencyModified => AttackLeniency + attackLeniencyModifier;
    public void ApplyAttackLeniencyModifier(float mod) => attackLeniencyModifier += mod;

    float defenseLeniencyModifier;
    public float DefenseLeniency => characterReference.defenseLeniency;
    public float DefenseLeniencyModifier => defenseLeniencyModifier;
    public float DefenseLeniencyModified => characterReference.defenseLeniency + defenseLeniencyModifier;
    public void ApplyDefenseLeniencyModifier(float mod) => defenseLeniencyModifier += mod;

    float healInModifier = 1;
    public float HealInModifier => healInModifier;
    public void ApplyHealInModifier(float mod) => healInModifier += mod;

    [SerializeField] [Range(0.02f, 1)] float critChance = 0.02f;

    /// <summary>
    /// The sum of the character's base crit chance and any modifiers before QTEs
    /// </summary>
    public virtual float CritChanceModified => critChance + critChanceModifier;

    float critChanceModifier = 0;

    public float CritChanceModifier => critChanceModifier;

    public abstract bool CanCrit { get; }

    [SerializeField] float critMultiplier = 3;
    float critDamageModifier;

    public float CritDamageModifier => critDamageModifier;
    public float CritDamageModified => 1 + critMultiplier + critDamageModifier;

    [SerializeField] float wait = 0.5f;
    public float Wait => wait;
    float waitModifier;
    public float WaitModifier => waitModifier;
    public float WaitModified => wait + waitModifier;
    float waitTimer;
    public float WaitTimer => waitTimer;
    public void IncrementWaitTimer()
    {
        waitTimer += WaitModified;
        OnWaitTimeChanged?.Invoke();
        OnCharacterWaitChanged?.Invoke(this);
    }

    [SerializeField] float waitLimit = 1;
    public float WaitLimit => waitLimit;
    float waitLimitModifier;
    public float WaitLimitModifier => waitLimitModifier;
    public float WaitLimitModified => waitLimit + waitLimitModifier;

    public float WaitPercentage => Mathf.Min(1, WaitTimer / WaitLimitModified);
    public bool IsOverWait => WaitPercentage >= 1;

    public void ResetWait() => waitTimer = 0;

    [SerializeField] protected Rarity rarity;
    public float RarityMultiplier => 1 + 0.5f * (int)rarity;

    public bool IsDodging;

    public bool IsDead => health <= 0;

    public bool CanDie = true;

    // If true, plays Attack Level 1 rather than Attack Execute State
    public bool EnhancedBasicAttack;

    List<AppliedEffect>[] effectMask = new List<AppliedEffect>[Enum.GetNames(typeof(EffectType)).Length];
    public void ApplyEffectMask(EffectType t, AppliedEffect effect) => effectMask[(int)t].Add(effect);

    [Header("Object References")]
    [SerializeField] protected Animator spriteAnim;

    [SerializeField] Transform effectRegion;
    public Transform EffectRegion => effectRegion;

    protected GameObject characterMesh;

    public GameObject CharacterMesh => characterMesh;

    protected Animator rigAnim;

    [SerializeField] protected GameObject skillParticles1;
    [SerializeField] protected GameObject skillParticles2;

    [SerializeField] protected GameObject deathParticles;

    [SerializeField] protected Renderer selectionCircle;
    [SerializeField] protected Vector2 selectionCircleScale = new Vector2(0.7f, 1f);

    [SerializeField] AnimationHelper animHelper;
    public AnimationHelper AnimHelper => animHelper;

    [SerializeField] protected GameObject canvasPrefab;

    public List<AppliedEffect> AppliedEffects = new List<AppliedEffect>();
    public Dictionary<BaseGameEffect, List<AppliedEffect>> EffectDictionary = new Dictionary<BaseGameEffect, List<AppliedEffect>>();

    protected List<GameSkill> gameSkills = new List<GameSkill>();
    public List<GameSkill> Skills => gameSkills;

    public bool CanUseSkill(int index)
    {
        return gameSkills[index].CanUse;
    }

    public bool CanUseSkill(GameSkill skill)
    {
        var resultSkill = gameSkills.Find(x => x == skill);
        return resultSkill != null && resultSkill.CanUse;
    }

    bool initialized;
    public bool Initialized => initialized;

    protected ViewportBillboard billBoard;

    protected bool usedSuperCritThisTurn;
    public bool UsedSuperCritThisTurn => usedSuperCritThisTurn;

    public Action<AppliedEffect> OnApplyGameEffect;
    public Action<AppliedEffect> OnEffectStacksChanged;
    public Action<AppliedEffect> OnRemoveGameEffect;

    public Action OnStartTurn;
    /// <summary>
    /// Happens immediately after OnStartTurn 
    /// Useful for GameEffects that are only applied in OnStartTurn 
    /// but don't immediately after activation
    /// </summary>
    public Action OnStartTurnLate;
    public Action OnEndTurn;
    public Action OnBeginSuperCrit;
    public Action<BaseCharacter> OnBeginAttack;
    /// <summary>
    /// bool qteSuccess 
    /// Value is flipped if it's the enemy's turn
    /// </summary>
    public Action<bool> OnExecuteAttack;
    /// <summary>
    /// 
    /// Invoked when this character deals damage to an enemy. 
    /// Can be invoked multiple times in one attack
    /// </summary>
    public Action<BaseCharacter> OnDealDamage;
    public Action OnHeal;
    public Action OnShielded;
    public Action OnShieldBroken;

    public Action OnSkillUsed;

    public Action OnWaitChanged;
    public Action OnWaitTimeChanged;
    public Action OnWaitLimitChanged;

    /// <summary>
    /// Only invoked when changing health through abnormal means
    /// </summary>
    public Action onSetHealth;

    /// <summary>
    /// float trueDamage, DamageStruct damage
    /// </summary>
    public Action<float, DamageStruct> OnTakeDamage;
    public Action<float> onConsumeHealth;

    public Action OnCharacterCritChanceChanged;

    public static Action<BaseCharacter> OnCharacterStartAttack;

    /// <summary>
    /// BaseCharacter character, bool success
    /// </summary>
    public static Action<BaseCharacter, bool> OnCharacterAttackBlocked;

    /// <summary>
    /// BaseCharacter character, DamageStruct damage
    /// </summary>
    public static Action<BaseCharacter, DamageStruct> OnCharacterReceivedDamage;
    public static Action<BaseCharacter, DamageStruct> OnCharacterConsumedHealth;

    public static Action<BaseCharacter, GameSkill> OnCharacterActivateSkill;

    public static Action<BaseCharacter, AppliedEffect> OnAppliedEffect;
    public static Action<BaseCharacter, AppliedEffect> OnRemoveEffect;

    public static Action<BaseCharacter> OnCharacterWaitChanged;

    public static Action<BaseCharacter> OnCharacterDeath;

    public static Action<BaseCharacter> OnSelectCharacter;
    public static Action<BaseCharacter> OnMouseDragUpdate;
    public static Action<BaseCharacter> OnMouseDragStop;

    /// <summary>
    /// BaseCharacter attacker, BaseCharacter defender
    /// </summary>
    public static Action<BaseCharacter, BaseCharacter> OnCharacterDealDamage;
    public static Action<BaseCharacter> OnCharacterExecuteAttack;
    public static Action<BaseCharacter, float> OnCharacterCritChanceReduced;

    public static AttackStruct IncomingAttack;
    public static DamageStruct IncomingDamage;

    public UnityEngine.Events.UnityEvent onDeath;

    public void SetCharacterAndRarity(CharacterObject newRef, Rarity newRarity)
    {
        characterReference = newRef;
        rarity = newRarity;
    }

    public virtual void InitializeWithInfo(int level, BattleState.State stateInfo = null)
    {
        if (characterReference == null) return;

        currentLevel = level;
        maxHealth = characterReference.GetMaxHealth(currentLevel, true) /* * RarityMultiplier*/;

        critChance = characterReference.critChance;
        critMultiplier = characterReference.critDamageMultiplier;
        wait = characterReference.wait;
        waitLimit = characterReference.waitLimit;

        Initialize();

        for (int i = 0; i < characterReference.skills.Length; i++)
        {
            GameSkill newSkill = new GameSkill();
            newSkill.InitWithSkill(characterReference.skills[i]);
            gameSkills.Add(newSkill);
        }

        if (stateInfo != null)
        {
            health = stateInfo.Health;
            waitTimer = stateInfo.WaitTimer;
            if (health == 0)
            {
                DieSilently();
                return;
            }
        }
        else
        {
            health = maxHealth;
        }

        StartCoroutine(CreateBillboardUI());
    }

    // This needs to happen after UI is made so UI can capture events
    public void InitializeAppliedEffects(BattleState.State stateInfo)
    {
        if (stateInfo != null)
        {
            for (int i = 0; i < stateInfo.Effects.Count; i++)
            {
                ApplyEffect(gameEffectLoader.DeserializeEffect(stateInfo.Effects[i], this));
            }
        }
    }

    void Initialize()
    {
        if (characterReference.characterRig != null)
        {
            var op = characterReference.characterRig.OperationHandle;
            AsyncOperationHandle<GameObject> loadOp;
            if (!op.IsValid())
            {
                loadOp = characterReference.characterRig.LoadAssetAsync<GameObject>();
            }
            else
            {
                loadOp = op.Convert<GameObject>();
            }

            if (!loadOp.IsDone)
            {
                loadOp.Completed += OnCharacterLoaded;
            }
            else
            {
                OnCharacterLoaded(loadOp);
            }
        }
    }

    protected abstract IEnumerator CreateBillboardUI();

    public void EnableBillboardUI()
    {
        if (billBoard)
        {
            billBoard.EnableWithSettings(sceneTweener.SceneCamera, CharacterMesh.transform);
        }
    }

    protected virtual void OnCharacterLoaded(AsyncOperationHandle<GameObject> obj)
    {
        characterMesh = Instantiate(obj.Result, transform);
        rigAnim = characterMesh.GetComponentInChildren<Animator>();
        animHelper = GetComponentInChildren<AnimationHelper>();
        initialized = true;
    }

    private void Awake()
    {
        for (int i = 0; i < effectMask.Length; i++)
        {
            effectMask[i] = new List<AppliedEffect>();
        }
    }

    protected virtual void OnEnable()
    {
        UIManager.OnAttackCommit += HideCharacterUI;
        BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] += OnStartPlayerTurn;
        OnEndTurn += CooldownSkills;

        GlobalEvents.OnCharacterFinishSuperCritical += OnCharacterFinishSuperCritical;
    }

    protected virtual void OnDisable()
    {
        UIManager.OnAttackCommit -= HideCharacterUI;
        BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] -= OnStartPlayerTurn;
        OnEndTurn -= CooldownSkills;

        GlobalEvents.OnCharacterFinishSuperCritical -= OnCharacterFinishSuperCritical;
    }

    protected virtual void OnMouseDrag()
    {
        OnMouseDragUpdate?.Invoke(this);
    }

    protected virtual void OnMouseUp()
    {
        OnMouseDragStop?.Invoke(this);
    }

    public abstract void ShowCharacterUI();

    public virtual void HideCharacterUI()
    {
        selectionCircle.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (selectionCircle.enabled)
        {
            float scale = Mathf.Lerp(selectionCircleScale.x, selectionCircleScale.y, Mathf.PingPong(Time.time / 2f, 1));
            selectionCircle.transform.localScale = new Vector3(scale, 1, scale);
        }
    }

    private void OnStartPlayerTurn()
    {
        usedSuperCritThisTurn = false;
        ShowCharacterUI();
    }

    public virtual void UseSuperCritical()
    {
        if (rigAnim)
        {
            rigAnim.Play("Super Critical");
        }

        usedSuperCritThisTurn = true;

        IncomingDamage.QTEValue = 1;
        IncomingDamage.IsCritical = true;
        IncomingDamage.IsSuperCritical = true;
        IncomingDamage.CritDamageModifier = CritDamageModified;
        animHelper.EnableCrits();

        OnBeginSuperCrit?.Invoke();
        GlobalEvents.OnCharacterUseSuperCritical?.Invoke(this);
    }

    protected virtual void HandleCrits()
    {
        if (IncomingDamage.IsCritical)
        {
            animHelper.EnableCrits();
        }

        IncomingDamage.CritDamageModifier = CritDamageModified;
    }

    public virtual void ExecuteAttack()
    {
        HandleCrits();

        QuickTimeBase.OnExecuteAnyQuickTime -= ExecuteAttack;
    }

    public void DealDamage(BaseCharacter target)
    {
        battleSystem.PerformAttack(this, target);
        OnDealDamage?.Invoke(target);
        OnCharacterDealDamage?.Invoke(this, target);
    }

    public virtual void BeginAttack(BaseCharacter target)
    {
        IncomingDamage = new DamageStruct();
        IncomingDamage.Percentage = 1;
        IncomingDamage.Source = this;

        var targetTransform = target.transform;
        if (CanCrit && !usedSuperCritThisTurn)
        {
            switch (Reference.superCritRange)
            {
                case AttackRange.CloseRange:
                    sceneTweener.MeleeMoveTo(transform, targetTransform);
                    break;
            }
            UseSuperCritical();
        }
        else
        {
            switch (IncomingAttack.attackRange)
            {
                case AttackRange.CloseRange:
                    sceneTweener.MeleeTweenTo(transform, targetTransform);
                    break;
                case AttackRange.LongRange:
                    sceneTweener.RangedTweenTo(CharacterMesh.transform, targetTransform);
                    break;
            }
            OnBeginAttack?.Invoke(target);
        }
    }

    public void FinishAttack()
    {
        switch (IncomingAttack.attackRange)
        {
            case AttackRange.CloseRange:
                sceneTweener.ReturnToPosition();
                break;
            case AttackRange.LongRange:
                sceneTweener.RotateBack();
                break;
        }

        if (IncomingDamage.IsCritical)
        {
            animHelper.DisableCrits();
        }

        battleSystem.FinishTurn();
    }

    public void FinishSuperCritAttack()
    {
        switch (Reference.tweenBackStyle)
        {
            case TweenBackType.None:
                sceneTweener.RotateBack();
                break;
            case TweenBackType.Teleport:
                sceneTweener.TeleportBackToPosition();
                break;
            case TweenBackType.Jump:
                sceneTweener.ReturnToPosition();
                break;
        }

        animHelper.DisableCrits();

        battleSystem.FinishTurn();
    }

    public void ReturnToIdle()
    {
        rigAnim.Play("Idle", 1, 0);
    }

    public void PlayReturnAnimation()
    {
        if (rigAnim)
        {
            rigAnim.SetTrigger("Jump Back");
        }
    }

    public virtual void UseSkill(GameSkill skill)
    {
        //stupid solution. TODO: rework this without index. Just use the skill itself
        var index = gameSkills.IndexOf(skill);

        currentSkill = gameSkills[index];
        StartCoroutine(SkillRoutine(index));
    }
    
    List<BaseCharacter> targets = new List<BaseCharacter>();

    GameSkill currentSkill;

    List<Action> onSkillFoundTargets = new List<Action>();
    public void RegisterOnSkillFoundTargets(Action newAction) => onSkillFoundTargets.Add(newAction);

    List<Action> onFinishApplyingSkillEffects = new List<Action>();
    public void RegisterOnFinishApplyingSkillEffects(Action newAction) => onFinishApplyingSkillEffects.Add(newAction);

    bool cancelSkill;

    public void AddSkillTarget(BaseCharacter character)
    {
        targets.Add(character);
    }

    public void ClearSkillTargets() => targets.Clear();

    IEnumerator SkillRoutine(int skillUsed)
    {
        cancelSkill = false;

        targets.Clear();

        switch (currentSkill.ReferenceSkill.targetMode)
        {
            case TargetMode.None:
            case TargetMode.Self:
                targets.Add(this);
                break;
            case TargetMode.OneAlly:
                switch (battleSystem.CurrentPhase)
                {
                    case BattlePhases.PlayerTurn:
                        PlayerCharacter.OnSelectPlayer += AddSkillTarget;
                        ui.EnterSkillTargetMode();
                        break;
                    case BattlePhases.EnemyTurn:
                        targets.Add(enemyController.RandomEnemy);
                        break;
                }
                break;
            case TargetMode.OneEnemy:
                switch (battleSystem.CurrentPhase)
                {
                    case BattlePhases.PlayerTurn:
                        targets.Add(battleSystem.ActiveEnemy);
                        break;
                    case BattlePhases.EnemyTurn:
                        targets.Add(battleSystem.EnemyAttackTarget);
                        break;
                }
                break;
            case TargetMode.AllAllies:
                switch (battleSystem.CurrentPhase)
                {
                    case BattlePhases.PlayerTurn:
                        for (int i = 0; i < battleSystem.PlayerCharacters.Length; i++)
                        {
                            if (!battleSystem.PlayerCharacters[i]) continue;
                            targets.Add(battleSystem.PlayerCharacters[i]);
                        }

                        break;
                    case BattlePhases.EnemyTurn:
                        for (int i = 0; i < enemyController.Enemies.Length; i++)
                        {
                            if (!enemyController.Enemies[i]) continue;
                            targets.Add(enemyController.Enemies[i]);
                        }

                        break;
                }
                break;
            case TargetMode.AllEnemies:
                switch (battleSystem.CurrentPhase)
                {
                    case BattlePhases.PlayerTurn:
                        for (int i = 0; i < enemyController.Enemies.Length; i++)
                        {
                            if (!enemyController.Enemies[i]) continue;
                            targets.Add(enemyController.Enemies[i]);
                        }

                        break;
                    case BattlePhases.EnemyTurn:
                        for (int i = 0; i < battleSystem.PlayerCharacters.Length; i++)
                        {
                            if (!battleSystem.PlayerCharacters[i]) continue;
                            targets.Add(battleSystem.PlayerCharacters[i]);
                        }

                        break;
                }
                break;
        }

        while (targets.Count == 0)
        {
            if (cancelSkill)
            {
                switch (currentSkill.ReferenceSkill.targetMode)
                {
                    case TargetMode.OneAlly:
                        PlayerCharacter.OnSelectPlayer -= AddSkillTarget;
                        break;
                    case TargetMode.OneEnemy:
                        EnemyCharacter.OnSelectedEnemyCharacterChange -= AddSkillTarget;
                        break;
                }

                ui.ExitSkillTargetMode();
                break;
            }

            yield return null;
        }

        if (!cancelSkill)
        {
            if (battleSystem.CurrentPhase == BattlePhases.PlayerTurn)
            {
                switch (currentSkill.ReferenceSkill.targetMode)
                {
                    case TargetMode.OneAlly:
                        PlayerCharacter.OnSelectPlayer -= AddSkillTarget;
                        ui.ExitSkillTargetMode();
                        break;
                    case TargetMode.OneEnemy:
                        EnemyCharacter.OnSelectedEnemyCharacterChange -= AddSkillTarget;
                        break;
                }
            }

            SkillExecuteRoutine(skillUsed);
        }

        onSkillFoundTargets.Clear();
    }

    public void SkillExecuteRoutine(int skillUsed)
    {
        Instantiate(skillParticles1, transform);
        //animHelper.RegisterOnFinishSkillAnimation(() => Instantiate(skillParticles2, transform));

        if (skillUsed == 0) JSAM.AudioManager.PlaySound(Reference.voiceFirstSkill);
        else JSAM.AudioManager.PlaySound(Reference.voiceSecondSkill);

        if (rigAnim)
        {
            if (Reference.hasAltSkillAnimation && skillUsed == 1)
            {
                rigAnim.Play("Skill Alt");
            }
            else rigAnim.Play("Skill");
        }
        else
        {
            spriteAnim.Play("Skill");
        }

        OnCharacterActivateSkill?.Invoke(this, currentSkill);
        OnSkillUsed?.Invoke();
        for (int i = 0; i < onSkillFoundTargets.Count; i++)
        {
            onSkillFoundTargets[i]();
        }
    }

    /// <summary>
    /// Cancels invocation of targetable skill
    /// </summary>
    public void CancelSkill()
    {
        cancelSkill = true;
    }

    public void ResolveSkill()
    {
        StartCoroutine(ActivateSkill());
    }

    public static void ApplyEffectToCharacter(EffectProperties props, BaseCharacter caster, BaseCharacter target)
    {
        if (!target) return;
        if (target.IsDead) return;
        ApplyEffectToCharacter(props, caster, new[] { target });
    }

    public static void ApplyEffectToCharacter(EffectProperties props, BaseCharacter caster, BaseCharacter[] targets)
    {
        if (targets == null) return;
        if (targets.Length == 0) return;
        if (props.effect.particlePrefab) Instantiate(props.effect.particlePrefab, targets[0].transform);
        // TODO: Uncomment me
        //newEffect.cachedValue = props.effect.Activate(caster, target, props.strength, props.customValues);

        EffectTextSpawner.Instance.SpawnEffectAt(props.effect, targets[0].transform);

        AppliedEffect newEffect = new AppliedEffect(caster, targets, props);

        targets[0].ApplyEffect(newEffect);

        OnAppliedEffect?.Invoke(targets[0], newEffect);
    }

    IEnumerator ActivateSkill()
    {
        currentSkill.BeginCooldown();

        for (int i = 0; i < currentSkill.ReferenceSkill.gameEffects.Length; i++)
        {
            EffectProperties effect = currentSkill.ReferenceSkill.gameEffects[i];
            switch (effect.targetOverride)
            {
                case TargetMode.None:
                    for (int j = 0; j < targets.Count; j++)
                    {
                        ApplyEffectToCharacter(effect, this, targets[j]);
                    }
                    break;
                case TargetMode.OneAlly:
                case TargetMode.OneEnemy:
                    Debug.LogError("TargetMode: " + TargetMode.OneAlly + " and " + TargetMode.OneEnemy +
                        " should not be used as overrides!");
                    break;
                case TargetMode.AllAllies:
                    switch (battleSystem.CurrentPhase)
                    {
                        case BattlePhases.PlayerTurn:
                            for (int j = 0; j < battleSystem.PlayerCharacters.Length; j++)
                            {
                                ApplyEffectToCharacter(effect, this, battleSystem.PlayerCharacters[j]);
                            }
                            break;
                        case BattlePhases.EnemyTurn:
                            for (int j = 0; j < enemyController.Enemies.Length; j++)
                            {
                                ApplyEffectToCharacter(effect, this, enemyController.Enemies[j]);
                            }
                            break;
                    }
                    break;
                case TargetMode.AllEnemies:
                    switch (battleSystem.CurrentPhase)
                    {
                        case BattlePhases.PlayerTurn:
                            for (int j = 0; j < enemyController.Enemies.Length; j++)
                            {
                                ApplyEffectToCharacter(effect, this, enemyController.Enemies[j]);
                            }
                            break;
                        case BattlePhases.EnemyTurn:
                            for (int j = 0; j < battleSystem.PlayerCharacters.Length; j++)
                            {
                                ApplyEffectToCharacter(effect, this, battleSystem.PlayerCharacters[j]);
                            }
                            break;
                    }
                    break;
                case TargetMode.Self:
                    ApplyEffectToCharacter(effect, this, this);
                    break;
            }

            GlobalEvents.OnGameEffectApplied?.Invoke(effect.effect);

            yield return new WaitForSeconds(sceneTweener.SkillEffectApplyDelay);
        }

        for (int i = 0; i < onFinishApplyingSkillEffects.Count; i++)
        {
            onFinishApplyingSkillEffects[i]?.Invoke();
        }

        onFinishApplyingSkillEffects.Clear();
    }

    public void ApplyEffect(AppliedEffect newEffect)
    {
        if (effectMask[(int)newEffect.referenceEffect.effectType].Count > 0)
        {
            var mask = effectMask[(int)newEffect.referenceEffect.effectType];
            var e = mask.GetLast();
            if (!e.Activate())
            {
                // Remove all instances of this effect in-case it masks other effects
                foreach (var em in effectMask)
                {
                    em.Remove(e);
                }
            }
            return;
        }

        bool apply = true;

        // Skip if this is just a one-time effect
        if (newEffect.remainingTurns != 0 || newEffect.remainingActivations != 0)
        {
            if (!EffectDictionary.ContainsKey(newEffect.referenceEffect))
            {
                EffectDictionary.Add(newEffect.referenceEffect, new List<AppliedEffect>());
            }

            if (newEffect.HasStacks)
            {
                if (EffectDictionary[newEffect.referenceEffect].Count > 0)
                {
                    EffectDictionary[newEffect.referenceEffect][0].Stacks += newEffect.Stacks;
                    newEffect = EffectDictionary[newEffect.referenceEffect][0];
                    apply = false;
                }
                else
                {
                    AppliedEffects.Add(newEffect);
                    EffectDictionary[newEffect.referenceEffect].Add(newEffect);
                }
            }
            else
            {
                AppliedEffects.Add(newEffect);
                EffectDictionary[newEffect.referenceEffect].Add(newEffect);
            }
        }
        
        if (apply)
        {
            newEffect.Apply();
        }
        OnApplyGameEffect?.Invoke(newEffect);
    }
    
    public void RemoveEffect(AppliedEffect effect, int stacks = 0)
    {
        if (effect.HasStacks)
        {
            var e = EffectDictionary[effect.referenceEffect][0];
            e.Stacks -= stacks;
            if (e.Stacks <= 0)
            {
                AppliedEffects.Remove(effect);
                EffectDictionary[effect.referenceEffect].Remove(effect);
                effect.OnExpire();
                OnRemoveGameEffect?.Invoke(effect);
            }
            else
            {
                OnEffectStacksChanged?.Invoke(effect);
            }
        }
        else
        {
            AppliedEffects.Remove(effect);
            EffectDictionary[effect.referenceEffect].Remove(effect);
            if (EffectDictionary[effect.referenceEffect].Count == 0)
            {
                EffectDictionary.Remove(effect.referenceEffect);
            }
            effect.OnExpire();
            OnRemoveGameEffect?.Invoke(effect);
        }
        OnRemoveEffect?.Invoke(this, effect);
    }

    public void RemoveAllEffectsOfType(BaseGameEffect effect, bool immediate = false)
    {
        var iterate = new List<AppliedEffect>(AppliedEffects);

        foreach (var item in iterate)
        {
            if (item.referenceEffect == effect)
            {
                AppliedEffects.Remove(item);
                OnRemoveGameEffect?.Invoke(item);
                OnRemoveEffect?.Invoke(this, item);
            }
        }
    }

    public void CooldownSkills()
    {
        // Tick cooldown on skills
        for (int i = 0; i < gameSkills.Count; i++)
        {
            gameSkills[i].Cooldown();
        }
    }

    public IEnumerator TickEffects(float delay)
    {
        var list = new List<AppliedEffect>(AppliedEffects);
        foreach (var item in list)
        {
            var e = item.referenceEffect;
            // Make sure the effect destroys itself
            if (e.tickPrefab) Instantiate(e.tickPrefab, transform);
            if (e.tickSound) JSAM.AudioManager.PlaySound(e.tickSound);

            item.Tick();

            yield return new WaitForSeconds(delay < e.TickAnimationTime ? delay : e.TickAnimationTime);
        }
    }

    public virtual void Heal(float healthGain)
    {
        healthGain *= HealInModifier;
        health += healthGain;
        OnHeal?.Invoke();
        EffectTextSpawner.Instance.SpawnHealNumberAt(healthGain, transform);
    }

    public virtual void GiveShield(float shieldGain, AppliedEffect effect)
    {
        shieldMods.Add(new StatModifier(shieldGain, effect));
        OnShielded?.Invoke();
    }

    public void ApplyAttackModifier(float modifier)
    {
        attackModifier += modifier;
    }

    public void ApplyDefenseModifier(float modifier)
    {
        defenseModifier += modifier;
    }

    public void ApplyDamageAbsorptionModifier(float modifier)
    {
        damageAbsorptionModifier += modifier;
    }

    public void ApplyCritChanceModifier(float modifier)
    {
        critChanceModifier += modifier;
        OnCharacterCritChanceChanged?.Invoke();
    }

    public void ApplyCritDamageModifier(float modifier)
    {
        critDamageModifier += modifier;
    }

    public void ApplyWaitModifier(float modifier)
    {
        waitModifier += modifier;
        OnWaitChanged?.Invoke();
    }

    public void ApplyWaitLimitModifier(float modifier)
    {
        waitLimitModifier += modifier;
        OnWaitLimitChanged?.Invoke();
    }

    public virtual void TakeDamage()
    {
        TakeDamage(IncomingDamage);
    }

    /// <summary>
    /// Consuming health should not kill you
    /// </summary>
    /// <param name="damage"></param>
    public virtual void ConsumeHealth(DamageStruct damage)
    {
        health = Mathf.Max(1, health - damage.TrueDamage);

        onConsumeHealth?.Invoke(damage.TrueDamage);
        OnCharacterConsumedHealth?.Invoke(this, damage);
    }

    public abstract float CalculateAttack(DamageStruct damage, float effectiveness);
    public abstract float CalculateDefense(DamageStruct damage);

    public virtual void TakeDamage(DamageStruct damage)
    {
        var myClass = characterReference.characterClass;
        CharacterClass attackerClass = CharacterClass.Offense;
        if (damage.Source != null)
        {
            if (IsDodging)
            {
                damage.Evaded = true;
                EffectTextSpawner.Instance.SpawnMissAt(transform);
                return;
            }

            attackerClass = damage.Source.Reference.characterClass;
            float effectiveness = DamageTriangle.GetEffectiveness(attackerClass, myClass);
            damage.Effectivity = DamageTriangle.EffectiveFloatToEnum(effectiveness);

            // TARGET ATK * DMG PERCENT * EFFECTIVENESS * (1 - QTE * TARGET DEF)
            damage.TrueDamage = damage.Source.CalculateAttack(damage, effectiveness);
        }

        if (damage.IsCritical) damage.TrueDamage *= damage.CritDamageModifier;

        var def = CalculateDefense(damage);

        float trueDamage = Mathf.Max(1, damage.TrueDamage * def);

        float shieldedDamage = 0;

        if (CurrentShield > 0)
        {
            float remainder = trueDamage;
            while (shieldMods.Count > 0 && remainder > 0)
            {
                var s = shieldMods.GetLast();
                if (s.Value >= remainder)
                {
                    s.Deduct(trueDamage);
                    remainder = 0;
                    shieldedDamage = trueDamage;
                }
                else
                {
                    shieldedDamage += s.Value;
                    remainder -= s.Value;
                    shieldMods.Remove(s);
                    if (s.ParentEffect != null)
                    {
                        s.ParentEffect.Remove();
                    }
                    else
                    {
                        Debug.LogWarning("Shield parent effect shouldn't be null");
                    }
                }
            }
            trueDamage = remainder;
        }

        health = Mathf.Max(0, health - trueDamage);
        damage.TrueDamage = trueDamage;
        damage.ShieldedDamage = shieldedDamage;

        bool blocked = true;

        if (IncomingDamage.Source)
        {
            if (IncomingDamage.QTEResult == QuickTimeBase.QTEResult.Perfect)
            {
                if (battleSystem.CurrentPhase == BattlePhases.PlayerTurn)
                    blocked = false;
            }
            else
            {
                if (battleSystem.CurrentPhase != BattlePhases.PlayerTurn)
                    blocked = false;
            }
        }
        else // Effect damage
        {
            blocked = false;
        }

        if (rigAnim)
        {
            if (blocked)
            {
                rigAnim.PlayInFixedTime("Block Reaction", 1, 0);
            }
            else if (trueDamage > 0)
            {
                rigAnim.PlayInFixedTime("Hit Reaction", 1, 0);
            }

            if (damage.Source != null && !damage.IsAOE)
            {
                characterMesh.transform.LookAt(damage.Source.transform);
            }
        }

        OnCharacterAttackBlocked?.Invoke(this, blocked);
        OnTakeDamage?.Invoke(trueDamage, damage);
        OnCharacterReceivedDamage?.Invoke(this, damage);

        TryToDie(damage);
    }

    protected void TryToDie(DamageStruct damage)
    {
        if (health == 0 && CanDie)
        {
            bool hasOnDeathEffect = false;

            foreach (var item in AppliedEffects)
            {
                if (!item.referenceEffect.activateOnDeath) continue;
                battleSystem.RegisterOnDeathEffect(item);
                hasOnDeathEffect = true;
            }

            // Don't die just yet
            if (hasOnDeathEffect) return;

            Die();

            if (rigAnim)
            {
                //PlayDamageShakeEffect(damage.damageNormalized);
                if (!damage.IsCritical)
                {
                    animHelper.EnableRagdoll();
                }
                else
                {
                    animHelper.EnableRagdollExplosion();
                }
            }
            else
            {
                if (!damage.IsCritical)
                {
                    PlayDamageShakeEffect(damage.QTEValue);
                    spriteAnim.Play("Death");
                }
            }
        }
    }

    public GameObject SpawnEffectPrefab(GameObject prefab, bool removeParent = false, bool destroyAutomatically = true)
    {
        effectRegion.rotation = characterMesh.transform.rotation;
        var newEffect = Instantiate(prefab, effectRegion.transform.position, effectRegion.rotation);
        //newEffect.transform.localEulerAngles = Vector3.zero;
        if (!removeParent)
        {
            newEffect.transform.SetParent(animHelper.SkeletonRoot);
        }

        if (destroyAutomatically)
        {
            Destroy(newEffect, 5);
        }

        return newEffect;
    }

    public void PlayDamageShakeEffect(float normalizedDamage) =>
        transform.DOShakePosition(0.75f, Mathf.Lerp(0.025f, 0.25f, normalizedDamage), 30, 90, false, true);

    public virtual void InvokeDeathEvents()
    {
        JSAM.AudioManager.PlaySound(BattleSceneSounds.KillSound);
    }

    public virtual void Die()
    {
    }

    /// <summary>
    /// For spawning corpses
    /// </summary>
    public virtual void DieSilently()
    {
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitUntil(() => initialized);
        animHelper.EnableRagdoll();
        if (billBoard) Destroy(billBoard.gameObject);
    }

    public float GetHealthPercent()
    {
        return Mathf.Min(1, health / maxHealth);
    }

    public void ShowSelectionCircle()
    {
        selectionCircle.enabled = true;
    }

    public virtual void HideSelectionPointer()
    {
        selectionCircle.enabled = false;
    }

    private void OnCharacterFinishSuperCritical(BaseCharacter obj)
    {
        ShowCharacterUI();
    }

    private void OnDestroy()
    {
        if (characterReference.characterRig.IsValid())
            characterReference.characterRig.ReleaseAsset();
    }

    [ContextMenu("Report Effects")]
    void ReportEffects()
    {
        foreach (var item in AppliedEffects)
        {
            Debug.Log(item.referenceEffect.name);
        }
    }

    [IngameDebugConsole.ConsoleMethod(nameof(BuddhaMode), "Gives all Characters an absurd amount of health")]
    public static void BuddhaMode()
    {
        foreach (var item in battleSystem.AllCharacters)
        {
            item.health += 999999;
            item.onSetHealth?.Invoke();
        }

        Debug.Log("Superhealed all Characters!");
    }
}