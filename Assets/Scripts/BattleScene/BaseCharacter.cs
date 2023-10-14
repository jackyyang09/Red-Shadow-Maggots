﻿using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Facade;

public struct DamageStruct
{
    /// <summary>
    /// The attacking character, if any
    /// </summary>
    public BaseCharacter source;

    /// <summary>
    /// Final damage passed to the character for damage calculation
    /// </summary>
    public float damage;

    /// <summery>
    /// If true, target evaded damage by means of a game effect
    /// </summary>
    public bool evaded;

    /// <summary>
    /// QuickTime value
    /// </summary>
    public float damageNormalized;

    /// <summary>
    /// Amount of damage to deal relative to attack stat
    /// </summary>
    public float percentage;

    public float barFill;
    public float critDamageModifier;
    public DamageType damageType;
    public DamageEffectivess effectivity;
    public QuickTimeBase.QTEResult qteResult;
    public bool isCritical;
    public bool isSuperCritical;
    public bool isAOE;
    public int chargeLevel;
}

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

    protected float shield;
    public float CurrentShield => shield;
    public float ShieldPercent => shield / maxHealth;

    protected float damageAbsorption = 1;

    float attackModifier = 1;
    /// <summary>
    /// Additive modifier from skills
    /// </summary>
    public float AttackModifier => attackModifier;
    public float Attack => characterReference.GetAttack(currentLevel);
    public float AttackModified => Attack * attackModifier;

    float defenseModifier = 1;
    public float Defense => characterReference.GetDefense(currentLevel);
    /// <summary>
    /// Additive modifier from skills
    /// </summary>
    public float DefenseModifier => defenseModifier;
    public float DefenseModified => Defense * DefenseModifier;

    public float attackLeniencyModifier;
    public float AttackLeniency => characterReference.attackLeniency + attackLeniencyModifier;

    public float defenseLeniencyModifier;
    public float DefenseLeniency => characterReference.defenseLeniency + defenseLeniencyModifier;

    [SerializeField] [Range(0.02f, 1)] float critChance = 0.02f;

    /// <summary>
    /// The sum of the character's base crit chance and any modifiers before QTEs
    /// </summary>
    public virtual float CritChanceModified
    {
        get { return critChance + critChanceModifier; }
    }

    [SerializeField] float critChanceModifier = 0;

    public float CritChanceModifier => critChanceModifier;

    public abstract bool CanCrit { get; }

    [SerializeField] float critMultiplier = 3;
    [SerializeField] float critDamageModifier = 0;

    public float CritDamageModified => critMultiplier + critDamageModifier;

    [SerializeField] protected Rarity rarity;
    public float RarityMultiplier => 1 + 0.5f * (int)rarity;

    public bool IsDodging;

    public bool IsDead => health <= 0;

    public bool CanDie = true;

    [Header("Object References")] [SerializeField]
    protected Animator spriteAnim;

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

    [SerializeField] AnimationHelper animHelper = null;
    public AnimationHelper AnimHelper => animHelper;

    [SerializeField] protected GameObject canvasPrefab = null;

    public Dictionary<BaseGameEffect, List<AppliedEffect>> AppliedEffects = new Dictionary<BaseGameEffect, List<AppliedEffect>>();

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
    public Action<AppliedEffect> OnRemoveGameEffect;
    public Action<AppliedEffect> OnRemoveGameEffectImmediate;

    public Action onHeal;
    public Action OnShielded;
    public Action OnShieldBroken;

    /// <summary>
    /// Only invoked when changing health through abnormal means
    /// </summary>
    public Action onSetHealth;

    public Action<float> onTakeDamage;

    public Action OnCharacterCritChanceChanged;

    public static Action<BaseCharacter> OnCharacterStartAttack;

    /// <summary>
    /// BaseCharacter character, bool success
    /// </summary>
    public static Action<BaseCharacter, bool> OnCharacterAttackBlocked;

    /// <summary>
    /// BaseCharacter character, float damage
    /// </summary>
    public static Action<BaseCharacter, float> OnCharacterReceivedDamage;

    public static Action<BaseCharacter, GameSkill> OnCharacterActivateSkill;

    public static Action<BaseCharacter> OnCharacterDeath;

    public static Action<BaseCharacter> OnSelectCharacter;
    public static Action<BaseCharacter, DamageStruct> OnCharacterExecuteAttack;
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

    protected virtual void OnEnable()
    {
        UIManager.OnAttackCommit += HideCharacterUI;
        BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] += OnStartPlayerTurn;
        BattleSystem.OnEndPhase[BattlePhases.EnemyTurn.ToInt()] += CooldownSkills;

        GlobalEvents.OnCharacterFinishSuperCritical += OnCharacterFinishSuperCritical;
    }

    protected virtual void OnDisable()
    {
        UIManager.OnAttackCommit -= HideCharacterUI;
        BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] -= OnStartPlayerTurn;
        BattleSystem.OnEndPhase[BattlePhases.EnemyTurn.ToInt()] -= CooldownSkills;

        GlobalEvents.OnCharacterFinishSuperCritical -= OnCharacterFinishSuperCritical;
    }

    public abstract void ShowCharacterUI();

    public abstract void HideCharacterUI();

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

        GlobalEvents.OnCharacterUseSuperCritical?.Invoke(this);
        usedSuperCritThisTurn = true;

        IncomingDamage.damageNormalized = 1;
        IncomingDamage.isCritical = true;
        IncomingDamage.isSuperCritical = true;
        IncomingDamage.critDamageModifier = CritDamageModified;
        animHelper.EnableCrits();
    }

    public virtual void ExecuteAttack()
    {
        CalculateAttackDamage();

        QuickTimeBase.OnExecuteAnyQuickTime -= ExecuteAttack;

        //if (rigAnim)
        //{
        //    switch (Reference.attackQteType)
        //    {
        //        case QTEType.SimpleBar:
        //            if (damage.isSuperCritical) rigAnim.SetInteger("Charge Level", 4);
        //            rigAnim.Play("Attack Execute");
        //            break;
        //        case QTEType.Hold:
        //            if (damage.qteResult != QuickTimeBase.QTEResult.Perfect)
        //            {
        //                rigAnim.SetInteger("Charge Level", 0);
        //            }
        //            else
        //            {
        //                rigAnim.SetInteger("Charge Level", damage.chargeLevel);
        //            }
        //            if (damage.isSuperCritical) rigAnim.SetInteger("Charge Level", 4);
        //            rigAnim.SetTrigger("Attack Execute");
        //            break;
        //    }
        //}
        //else
        //{
        //    spriteAnim.Play("Attack Execute");
        //}
    }

    public void DealDamage(float percentage = 1)
    {
        IncomingDamage.percentage = percentage;
        IncomingDamage.isAOE = false;
        BattleSystem.Instance.AttackTarget();
        OnCharacterExecuteAttack?.Invoke(this, IncomingDamage);
    }

    public void DealAOEDamage(float percentage = 1)
    {
        IncomingDamage.percentage = percentage;
        IncomingDamage.isAOE = true;
        BattleSystem.Instance.AttackAOE();
        OnCharacterExecuteAttack?.Invoke(this, IncomingDamage);
    }

    public virtual void BeginAttack(Transform target)
    {
        IncomingDamage = new DamageStruct();
        IncomingDamage.percentage = 1;
        IncomingDamage.source = this;

        if (CanCrit && !usedSuperCritThisTurn)
        {
            switch (Reference.superCritRange)
            {
                case AttackRange.CloseRange:
                    sceneTweener.MeleeMoveTo(transform, target);
                    break;
            }
            UseSuperCritical();
        }
        else
        {
            switch (IncomingAttack.attackRange)
            {
                case AttackRange.CloseRange:
                    sceneTweener.MeleeTweenTo(transform, target);
                    break;
                case AttackRange.LongRange:
                    sceneTweener.RangedTweenTo(CharacterMesh.transform, target);
                    break;
            }
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

        if (IncomingDamage.isCritical)
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

    public void PlayReturnAnimation()
    {
        if (rigAnim)
        {
            rigAnim.SetTrigger("Jump Back");
        }
    }

    public virtual void CalculateAttackDamage()
    {
        float finalCritChance = CritChanceModified;
        // Add an additional crit chance factor on successful attack QTE
        if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn)
        {
            if (IncomingDamage.qteResult == QuickTimeBase.QTEResult.Perfect)
            {
                finalCritChance += BattleSystem.QuickTimeCritModifier;
            }
        }

        OnCharacterCritChanceChanged?.Invoke();
        IncomingDamage.isCritical = UnityEngine.Random.value < finalCritChance;
        if (IncomingDamage.isCritical)
        {
            IncomingDamage.isSuperCritical = finalCritChance >= 1.0f && !UsedSuperCritThisTurn;
            animHelper.EnableCrits();
        }

        IncomingDamage.critDamageModifier = CritDamageModified;
    }

    public virtual float CalculateDefenseDamage(float damage)
    {
        return Mathf.Max(1, damage * damageAbsorption);
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

        switch (currentSkill.referenceSkill.targetMode)
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
                switch (currentSkill.referenceSkill.targetMode)
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
            switch (currentSkill.referenceSkill.targetMode)
            {
                case TargetMode.OneAlly:
                    PlayerCharacter.OnSelectPlayer -= AddSkillTarget;
                    ui.ExitSkillTargetMode();
                    break;
                case TargetMode.OneEnemy:
                    EnemyCharacter.OnSelectedEnemyCharacterChange -= AddSkillTarget;
                    break;
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
        if (target.IsDead) return;
        if (props.effect.particlePrefab) Instantiate(props.effect.particlePrefab, target.transform);
        props.effect.Activate(caster, target, props.strength, props.customValues);

        EffectTextSpawner.Instance.SpawnEffectAt(props.effect, target.transform);

        if (props.effectDuration == 0) return;

        AppliedEffect newEffect = new AppliedEffect();

        newEffect.caster = caster;
        newEffect.target = target;
        newEffect.referenceEffect = props.effect;
        newEffect.remainingTurns = props.effectDuration;
        newEffect.strength = props.strength;
        newEffect.customValues = props.customValues;
        newEffect.description =
            props.effect.GetEffectDescription(TargetMode.Self, props.strength, props.customValues,
                props.effectDuration);
        newEffect.description = newEffect.description.Remove(newEffect.description.IndexOf("("));
        target.ApplyEffect(newEffect);
    }

    IEnumerator ActivateSkill()
    {
        currentSkill.BeginCooldown();

        for (int i = 0; i < currentSkill.referenceSkill.gameEffects.Length; i++)
        {
            EffectProperties effect = currentSkill.referenceSkill.gameEffects[i];
            switch (effect.targetOverride)
            {
                case TargetMode.None:
                    for (int j = 0; j < targets.Count; j++)
                    {
                        ApplyEffectToCharacter(effect, this, targets[j]);
                    }

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

            yield return new WaitForSeconds(0.75f);
        }

        for (int i = 0; i < onFinishApplyingSkillEffects.Count; i++)
        {
            onFinishApplyingSkillEffects[i]?.Invoke();
        }

        onFinishApplyingSkillEffects.Clear();
    }

    public void ApplyEffect(AppliedEffect newEffect)
    {
        // TODO: Make some effects stack-able and some not
        // if (newEffect.referenceEffect.canStack)
        if (!AppliedEffects.ContainsKey(newEffect.referenceEffect))
        {
            AppliedEffects.Add(newEffect.referenceEffect, new List<AppliedEffect>());
        }

        AppliedEffects[newEffect.referenceEffect].Add(newEffect);
        OnApplyGameEffect?.Invoke(newEffect);
    }

    public void RemoveAllEffectsOfType(BaseGameEffect effect, bool immediate = false)
    {
        if (!AppliedEffects.ContainsKey(effect)) return;

        for (int i = AppliedEffects[effect].Count - 1; i > -1; i--)
        {
            if (immediate)
            {
                OnRemoveGameEffectImmediate?.Invoke(AppliedEffects[effect][i]);
            }
            else
            {
                OnRemoveGameEffect?.Invoke(AppliedEffects[effect][i]);
            }
            AppliedEffects[effect].RemoveAt(i);
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

    public void TickEffect(BaseGameEffect effect)
    {
        if (AppliedEffects[effect].Count == 1)
        {
            if (!AppliedEffects[effect][0].Tick()) // Check if still active after ticking
            {
                // Remove the effect
                OnRemoveGameEffect?.Invoke(AppliedEffects[effect][0]);
                AppliedEffects[effect].RemoveAt(0);
            }
        }
        else if (AppliedEffects[effect].Count > 1)
        {
            var newList = effect.TickMultiple(this, this, AppliedEffects[effect]);
            var diff = AppliedEffects[effect].Except(newList).ToList();

            for (int i = 0; i < diff.Count; i++)
            {
                OnRemoveGameEffect?.Invoke(diff.ElementAt(i));
            }

            AppliedEffects[effect] = newList;
        }

        if (AppliedEffects[effect].Count == 0)
        {
            AppliedEffects.Remove(effect);
        }
    }

    public virtual void Heal(float healthGain)
    {
        health = Mathf.Min(health + healthGain, maxHealth);
        onHeal?.Invoke();
        onSetHealth?.Invoke();
        EffectTextSpawner.Instance.SpawnHealNumberAt(healthGain, transform);
    }

    public virtual void GiveShield(float shieldGain)
    {
        shield = Mathf.Min(shield + shieldGain, maxHealth);
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
        damageAbsorption += modifier;
    }

    public void ApplyCritChanceModifier(float modifier)
    {
        critChanceModifier += Mathf.Max(modifier, 0);
        OnCharacterCritChanceChanged?.Invoke();
    }

    public void RemoveCritChanceModifier(float modifier)
    {
        critChanceModifier -= Mathf.Max(modifier, 0);
        OnCharacterCritChanceReduced?.Invoke(this, modifier);
    }

    public void ApplyCritDamageModifier(float modifier)
    {
        critDamageModifier += modifier;
    }

    public virtual void TakeDamage()
    {
        TakeDamage(IncomingDamage);
    }

    public virtual void HurtSilent(DamageStruct damage)
    {

    }

    public virtual void TakeDamage(DamageStruct damage)
    {
        var myClass = characterReference.characterClass;
        CharacterClass attackerClass = CharacterClass.Offense;
        if (damage.source != null)
        {
            if (IsDodging)
            {
                damage.evaded = true;
                EffectTextSpawner.Instance.SpawnMissAt(transform.parent);
                return;
            }

            attackerClass = damage.source.Reference.characterClass;
            float effectiveness = DamageTriangle.GetEffectiveness(attackerClass, myClass);
            damage.effectivity = DamageTriangle.EffectiveFloatToEnum(effectiveness);

            damage.damage = damage.damageNormalized *
                            damage.source.AttackModified *
                            damage.percentage *
                            effectiveness;
        }

        if (damage.isCritical) damage.damage *= damage.critDamageModifier;

        float trueDamage = CalculateDefenseDamage(damage.damage);
        float shieldedDamage = 0;

        if (shield > 0)
        {
            var shielding = shield / (1 - DefenseModified);
            if (shielding >= trueDamage)
            {
                shield -= trueDamage * (1 - DefenseModified);
                shieldedDamage = trueDamage;
                trueDamage = 0;
            }
            else
            {
                shieldedDamage = shield;
                shield = 0;
                trueDamage -= shielding;
            }
        }

        health = Mathf.Clamp(health - trueDamage, 0, maxHealth);
        damage.damage = trueDamage;

        DamageNumberSpawner.Instance.SpawnDamageNumberAt(transform, damage, (int)shieldedDamage);

        if (trueDamage > 0)
        {
            if (shieldedDamage > 0)
            {
                DamageNumberSpawner.Instance.SpawnDamageNumberAt(transform, damage, (int)shieldedDamage);
                DamageNumberSpawner.Instance.SpawnDamageNumberDelayed(transform, damage, 0);
                OnShieldBroken?.Invoke();
            }
            else
            {
                DamageNumberSpawner.Instance.SpawnDamageNumberAt(transform, damage, 0);
            }
        }
        else if (shieldedDamage > 0)
        {
            DamageNumberSpawner.Instance.SpawnDamageNumberAt(transform, damage, (int)shieldedDamage);
        }

        if (rigAnim)
        {
            bool blocked = true;

            if (IncomingDamage.qteResult == QuickTimeBase.QTEResult.Perfect)
            {
                if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn)
                    blocked = false;
            }
            else
            {
                if (BattleSystem.Instance.CurrentPhase != BattlePhases.PlayerTurn)
                    blocked = false;
            }

            if (blocked)
            {
                rigAnim.Play("Block Reaction");
            }
            else if (trueDamage > 0)
            {
                rigAnim.Play("Hit Reaction");
            }
            OnCharacterAttackBlocked?.Invoke(this, blocked);

            if (damage.source != null && !damage.isAOE)
            {
                characterMesh.transform.LookAt(damage.source.transform);
            }
        }

        onTakeDamage?.Invoke(trueDamage);
        OnCharacterReceivedDamage?.Invoke(this, trueDamage + shieldedDamage);

        if (health == 0 && CanDie)
        {
            if (rigAnim)
            {
                //PlayDamageShakeEffect(damage.damageNormalized);
                if (!damage.isCritical)
                {
                    animHelper.EnableRagdoll();
                }
                else
                {
                    animHelper.EnableRagdollExplosion();
                }

                Die();
            }
            else
            {
                if (!damage.isCritical)
                {
                    PlayDamageShakeEffect(damage.damageNormalized);
                    spriteAnim.Play("Death");
                    Die();
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
        return health / maxHealth;
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
}