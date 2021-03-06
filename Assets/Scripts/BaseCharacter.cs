﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public struct DamageStruct
{
    /// <summary>
    /// Final damage passed to the character for damage calculation
    /// </summary>
    public float damage;
    /// <summary>
    /// QuickTime value
    /// </summary>
    public float damageNormalized;
    public float barFill;
    public DamageType damageType;
    public DamageEffectivess effectivity;
    public QuickTimeBase.QTEResult qteResult;
    public bool isCritical;
    public int chargeLevel;
}

public abstract class BaseCharacter : MonoBehaviour
{
    [SerializeField]
    protected CharacterObject characterReference;
    public CharacterObject Reference
    {
        get
        {
            return characterReference;
        }
    }

    [SerializeField]
    float health;
    public float CurrentHealth
    {
        get
        {
            return health;
        }
    }

    [SerializeField]
    float maxHealth;
    public float MaxHealth
    {
        get
        {
            return maxHealth;
        }
    }

    [SerializeField]
    float attack;

    /// <summary>
    /// Additive modifier from skills
    /// </summary>
    [SerializeField]
    float attackModifier;
    public float AttackModifier
    {
        get
        {
            return attackModifier;
        }
    }

    /// <summary>
    /// Additive modifier from skills
    /// </summary>
    [SerializeField]
    float defenseModifier;

    [SerializeField] [Range(0.02f, 1)] float critChance = 0.02f;

    [SerializeField] float critChanceModifier = 0;
    public float CritChanceModifier
    {
        get
        {
            return critChanceModifier;
        }
    }

    [SerializeField] float critMultiplier = 3;
    [SerializeField] float critDamageModifier = 0;

    [SerializeField]
    AttackRange range;

    [SerializeField] Rarity rarity;

    public bool IsDead
    {
        get
        {
            return !(health > 0);
        }
    }

    [Header("Object References")]

    [SerializeField] protected CharacterCardHolder cardHolder;

    [SerializeField] protected Rigidbody rigidBody;

    [SerializeField] protected Animator spriteAnim;

    [SerializeField] Transform effectRegion = null;

    protected GameObject characterMesh;
    public GameObject CharacterMesh { get { return characterMesh; } }
    protected Animator rigAnim;

    [SerializeField] protected GameObject skillParticles1;
    [SerializeField] protected GameObject skillParticles2;

    [SerializeField] protected GameObject deathParticles;

    [SerializeField] protected UnityEngine.UI.Image selectionPointer;

    [SerializeField] AnimationHelper animHelper = null;
    public AnimationHelper AnimHelper { get { return animHelper; } }

    public List<AppliedEffect> AppliedEffects { get; } = new List<AppliedEffect>();

    List<GameSkill> gameSkills = new List<GameSkill>();

    protected Animator anim;

    public Action<BaseGameEffect> onApplyGameEffect;
    public Action<BaseGameEffect> onRemoveGameEffect;

    public Action onHeal;
    public Action onTakeDamage;

    public static DamageStruct incomingDamage;

    public UnityEngine.Events.UnityEvent onDeath;

    public void SetCharacterAndRarity(CharacterObject newRef, Rarity newRarity)
    {
        characterReference = newRef;
        rarity = newRarity;

        cardHolder.SetCharacterAndRarity(newRef, newRarity);
        ApplyCharacterStats();
    }

    public void ApplyCharacterStats()
    {
        if (characterReference == null) return;

        float rarityMultiplier = 1 + 0.5f * (int)rarity;

        maxHealth = characterReference.maxHealth * rarityMultiplier;
        attack = characterReference.attack * rarityMultiplier;
        critChance = characterReference.critChance;
        critMultiplier = characterReference.critDamageMultiplier;

        range = characterReference.range;

        health = maxHealth;
    }

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();

        ApplyCharacterStats();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = maxHealth;

        for (int i = 0; i < characterReference.skills.Length; i++)
        {
            GameSkill newSkill = new GameSkill();
            newSkill.InitWithSkill(characterReference.skills[i]);
            gameSkills.Add(newSkill);
        }

        if (characterReference.characterRig)
        {
            characterMesh = Instantiate(characterReference.characterRig, transform);
            rigAnim = characterMesh.GetComponentInChildren<Animator>();
            animHelper = GetComponentInChildren<AnimationHelper>();
        }
    }

    protected virtual void OnEnable()
    {
        UIManager.onAttackCommit += HideCharacterUI;
        BattleSystem.onStartPlayerTurn += ShowCharacterUI;
        BattleSystem.onStartPlayerTurn += TickEffects;
    }

    protected virtual void OnDisable()
    {
        UIManager.onAttackCommit -= HideCharacterUI;
        BattleSystem.onStartPlayerTurn -= ShowCharacterUI;
        BattleSystem.onStartPlayerTurn -= TickEffects;
    }

    public abstract void ShowCharacterUI();

    public abstract void HideCharacterUI();

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public void PlayAttackAnimation()
    {
        GlobalEvents.OnCharacterStartAttack?.Invoke(this);
        QuickTimeBase.onExecuteQuickTime += ExecuteAttack;
        if (rigAnim)
        {
            rigAnim.Play("Attack Windup");
        }
        else
        {
            spriteAnim.Play("Attack Windup");
        }
    }

    public virtual void ExecuteAttack(DamageStruct damage)
    {
        CalculateAttackDamage(damage);

        QuickTimeBase.onExecuteQuickTime -= ExecuteAttack;

        if (rigAnim)
        {
            switch (Reference.attackQteType)
            {
                case QTEType.SimpleBar:
                    rigAnim.Play("Attack Execute");
                    break;
                case QTEType.Hold:
                    if (damage.qteResult == QuickTimeBase.QTEResult.Perfect)
                    {
                        rigAnim.SetInteger("Charge Level", 0);
                    }
                    else
                    {
                        rigAnim.SetInteger("Charge Level", damage.chargeLevel);
                    }
                    rigAnim.SetTrigger("Attack Execute");
                    break;
            }
        }
        else
        {
            spriteAnim.Play("Attack Execute");
        }
    }

    public void DealDamage()
    {
        BattleSystem.instance.AttackTarget(incomingDamage);
        GlobalEvents.OnCharacterExecuteAttack?.Invoke(this, incomingDamage);
    }

    public void FinishAttack()
    {
        switch (Reference.range)
        {
            case AttackRange.CloseRange:
                SceneTweener.Instance.ReturnToPosition();
                break;
            case AttackRange.LongRange:
                SceneTweener.Instance.RotateBack();
                break;
        }
    }

    public void PlayReturnAnimation()
    {
        if (rigAnim)
        {
            rigAnim.SetTrigger("Jump Back");
        }
    }

    public virtual void CalculateAttackDamage(DamageStruct damageStruct)
    {
        var playerClass = characterReference.characterClass;
        var enemyClass = BattleSystem.instance.GetOpposingCharacter().characterReference.characterClass;

        float effectiveness = DamageTriangle.GetEffectiveness(playerClass, enemyClass);
        damageStruct.effectivity = DamageTriangle.EffectiveFloatToEnum(effectiveness);

        float finalCritChance = critChance + critChanceModifier;
        if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn)
            finalCritChance += Convert.ToInt16(damageStruct.qteResult == QuickTimeBase.QTEResult.Perfect) * BattleSystem.QuickTimeCritModifier;
        damageStruct.isCritical = (UnityEngine.Random.value < finalCritChance);

        damageStruct.damage = damageStruct.damageNormalized * (attack + attackModifier) * effectiveness;
        if (damageStruct.isCritical) damageStruct.damage *= (critMultiplier + critDamageModifier);

        incomingDamage = damageStruct;
    }

    public virtual float CalculateDefenseDamage(float damage)
    {
        return Mathf.Clamp(damage - (damage * defenseModifier), 1, damage);
    }

    public void UseSkill(int index)
    {
        currentSkill = gameSkills[index];
        StartCoroutine(SkillRoutine(index));
    }

    public bool CanUseSkill(int index)
    {
        return gameSkills[index].CanUse;
    }

    public GameSkill GetSkill(int index)
    {
        return gameSkills[index];
    }

    List<BaseCharacter> targets;

    GameSkill currentSkill;

    List<Action> onSkillFoundTargets = new List<Action>();
    public void RegisterOnSkillFoundTargets(Action newAction) => onSkillFoundTargets.Add(newAction);

    List<Action> onFinishApplyingSkillEffects = new List<Action>();
    public void RegisterOnFinishApplyingSkillEffects(Action newAction) => onFinishApplyingSkillEffects.Add(newAction);

    bool cancelSkill;

    IEnumerator SkillRoutine(int skillUsed)
    {
        cancelSkill = false;

        targets = new List<BaseCharacter>();

        Action<BaseCharacter> addTarget = (x) => targets.Add(x);

        switch (currentSkill.referenceSkill.targetMode)
        {
            case TargetMode.None:
                targets.Add(this);
                break;
            case TargetMode.OneAlly:
                PlayerCharacter.onSelectPlayer += addTarget;
                UIManager.instance.EnterSkillTargetMode();
                break;
            case TargetMode.OneEnemy:
                EnemyCharacter.onSelectedEnemyCharacterChange += addTarget;
                break;
            case TargetMode.AllAllies:
                for (int i = 0; i < BattleSystem.instance.PlayerCharacters.Count; i++)
                {
                    targets.Add(BattleSystem.instance.PlayerCharacters[i]);
                }
                break;
            case TargetMode.AllEnemies:
                for (int i = 0; i < EnemyController.instance.enemies.Count; i++)
                {
                    targets.Add(EnemyController.instance.enemies[i]);
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
                        PlayerCharacter.onSelectPlayer -= addTarget;
                        break;
                    case TargetMode.OneEnemy:
                        EnemyCharacter.onSelectedEnemyCharacterChange -= addTarget;
                        break;
                }
                UIManager.instance.ExitSkillTargetMode();
                break;
            }
            yield return null;
        }

        if (!cancelSkill)
        {
            Instantiate(skillParticles1, transform);
            //animHelper.RegisterOnFinishSkillAnimation(() => Instantiate(skillParticles2, transform));

            switch (currentSkill.referenceSkill.targetMode)
            {
                case TargetMode.OneAlly:
                    PlayerCharacter.onSelectPlayer -= addTarget;
                    UIManager.instance.ExitSkillTargetMode();
                    break;
                case TargetMode.OneEnemy:
                    EnemyCharacter.onSelectedEnemyCharacterChange -= addTarget;
                    break;
            }

            if (skillUsed == 0) JSAM.AudioManager.instance.PlaySoundInternal(Reference.voiceFirstSkill);
            else JSAM.AudioManager.instance.PlaySoundInternal(Reference.voiceSecondSkill);

            if (rigAnim)
            {
                rigAnim.Play("Skill");
            }
            else
            { 
                spriteAnim.Play("Skill");
            }

            GlobalEvents.OnCharacterActivateSkill?.Invoke(this, currentSkill);
            for (int i = 0; i < onSkillFoundTargets.Count; i++)
            {
                onSkillFoundTargets[i]();
            }
        }
        onSkillFoundTargets.Clear();
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

    void ApplyEffectToCharacter(SkillObject.EffectProperties effectAndDuration, BaseCharacter character)
    {
        Instantiate(effectAndDuration.effect.particlePrefab, character.transform);
        effectAndDuration.effect.Activate(character, effectAndDuration.strength, effectAndDuration.customValues);
        if (effectAndDuration.effectDuration == 0) return;

        AppliedEffect newEffect = new AppliedEffect();

        newEffect.referenceEffect = effectAndDuration.effect;
        newEffect.remainingTurns = effectAndDuration.effectDuration;
        newEffect.strength = effectAndDuration.strength;
        newEffect.customValues = effectAndDuration.customValues;
        character.ApplyEffect(newEffect);
    }

    IEnumerator ActivateSkill()
    {
        currentSkill.BeginCooldown();

        for (int i = 0; i < currentSkill.referenceSkill.gameEffects.Length; i++)
        {
            SkillObject.EffectProperties effect = currentSkill.referenceSkill.gameEffects[i];
            if (effect.effect.targetOverride != TargetMode.None)
            {
                switch (effect.effect.targetOverride)
                {
                    case TargetMode.AllAllies:
                        for (int j = 0; j < BattleSystem.instance.PlayerCharacters.Count; j++)
                        {
                            ApplyEffectToCharacter(effect, BattleSystem.instance.PlayerCharacters[j]);
                        }
                        break;
                    case TargetMode.AllEnemies:
                        for (int j = 0; j < EnemyController.instance.enemies.Count; j++)
                        {
                            ApplyEffectToCharacter(effect, EnemyController.instance.enemies[j]);
                        }
                        break;
                    case TargetMode.Self:
                        ApplyEffectToCharacter(effect, this);
                        break;
                }
            }
            else
            {
                for (int j = 0; j < targets.Count; j++)
                {
                    ApplyEffectToCharacter(effect, targets[j]);
                }
            }

            switch (effect.effect.effectType)
            {
                case EffectType.Heal:
                    JSAM.AudioManager.PlaySound(JSAM.Sounds.HealApplied);
                    break;
                case EffectType.Buff:
                    JSAM.AudioManager.PlaySound(JSAM.Sounds.BuffApplied);
                    break;
                case EffectType.Debuff:
                    JSAM.AudioManager.PlaySound(JSAM.Sounds.DebuffApplied);
                    break;
            }

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
        AppliedEffects.Add(newEffect);
        EffectTextSpawner.instance.SpawnEffectAt(newEffect.referenceEffect, transform);
        onApplyGameEffect?.Invoke(newEffect.referenceEffect);
    }

    public void TickEffects()
    {
        List<AppliedEffect> effectsToRemove = new List<AppliedEffect>();
        // Tick cooldown on skills
        for (int i = 0; i < gameSkills.Count; i++)
        {
            gameSkills[i].Tick();
        }

        for (int i = 0; i < AppliedEffects.Count; i++)
        {
            if (!AppliedEffects[i].Tick(this))
            {
                // Queue removal of effect
                effectsToRemove.Add(AppliedEffects[i]);
            }
        }

        for (int i = 0; i < effectsToRemove.Count; i++)
        {
            // Remove the effect
            AppliedEffects.Remove(effectsToRemove[i]);
            onRemoveGameEffect?.Invoke(effectsToRemove[i].referenceEffect);
        }
    }

    public virtual void Heal(float healthGain)
    {
        health = Mathf.Clamp(health + healthGain, 0, maxHealth);
        onHeal?.Invoke();
        EffectTextSpawner.instance.SpawnHealNumberAt(healthGain, transform);
    }

    public void ApplyAttackModifier(float modifier)
    {
        attackModifier += modifier;
    }

    public void ApplyDefenseModifier(float modifier)
    {
        defenseModifier += modifier;
    }

    public void ApplyCritChanceModifier(float modifier)
    {
        critChanceModifier += modifier;
    }

    public void ApplyCritDamageModifier(float modifier)
    {
        critDamageModifier += modifier;
    }

    public virtual void TakeDamage(DamageStruct damage)
    {
        float trueDamage = CalculateDefenseDamage(damage.damage);
        health = Mathf.Clamp(health - trueDamage, 0, maxHealth);

        DamageNumberSpawner.instance.SpawnDamageNumberAt(transform.parent, damage);
        if (rigAnim)
        {
            if (incomingDamage.qteResult == QuickTimeBase.QTEResult.Perfect)
            {
                if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn)
                    rigAnim.Play("Hit Reaction");
                else
                    rigAnim.Play("Block Reaction");
            }
            else
            {
                if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn)
                    rigAnim.Play("Block Reaction");
                else
                    rigAnim.Play("Hit Reaction");
            }
        }
        else
        {
            spriteAnim.Play("Hurt");
        }

        onTakeDamage?.Invoke();
        GlobalEvents.OnCharacterAttacked?.Invoke(this);

        if (health == 0)
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

    public void SpawnEffectPrefab(GameObject prefab)
    {
        effectRegion.rotation = characterMesh.transform.rotation;
        var newEffect = Instantiate(prefab, effectRegion);
        newEffect.transform.localEulerAngles = Vector3.zero;
        newEffect.transform.SetParent(animHelper.transform.GetChild(0).GetChild(0));
        Destroy(newEffect, 5);
    }

    public void PlayDamageShakeEffect(float normalizedDamage) => transform.DOShakePosition(0.75f, Mathf.Lerp(0.025f, 0.25f, normalizedDamage), 30, 90, false, true);

    public abstract void InvokeDeathEvents();
    public abstract void Die();

    public float GetHealthPercent()
    {
        return health / maxHealth;
    }

    public void ShowSelectionPointer()
    {
        selectionPointer.enabled = true;
    }

    public virtual void HideSelectionPointer()
    {
        selectionPointer.enabled = false;
    }
}
