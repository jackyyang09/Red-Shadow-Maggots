using System.Collections;
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
    public DamageType damageType;
    public DamageEffectivess effectivity;
    public bool isCritical;
    /// <summary>
    /// Did the player hit the red zone?
    /// </summary>
    public bool quickTimeSuccess;
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

    [Header("Death Effect Properties")]

    [SerializeField] protected float knockbackForce = 100;
    [SerializeField] protected Transform knockbackSource;
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

    [SerializeField] protected GameObject skillParticles;

    [SerializeField] protected GameObject deathParticles;

    [SerializeField] protected UnityEngine.UI.Image selectionPointer;

    public List<AppliedEffect> AppliedEffects { get; } = new List<AppliedEffect>();

    List<GameSkill> gameSkills = new List<GameSkill>();

    protected Animator anim;

    public Action<BaseGameEffect> onApplyGameEffect;
    public Action<BaseGameEffect> onRemoveGameEffect;

    public Action onHeal;
    public Action onTakeDamage;

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

        foreach (SkillObject skillObject in characterReference.skills)
        {
            GameSkill newSkill = new GameSkill();
            newSkill.InitWithSkill(skillObject);
            gameSkills.Add(newSkill);
        }
    }

    protected virtual void OnEnable()
    {
        UIManager.onAttackCommit += HideCharacterUI;
        BattleSystem.onStartPlayerTurn += ShowCharacterUI;
        BattleSystem.onStartPlayerTurn += TickEffects;
        GlobalEvents.onModifyGameSpeed += GameSpeedChanged;
    }

    protected virtual void OnDisable()
    {
        UIManager.onAttackCommit -= HideCharacterUI;
        BattleSystem.onStartPlayerTurn -= ShowCharacterUI;
        BattleSystem.onStartPlayerTurn -= TickEffects;
        GlobalEvents.onModifyGameSpeed -= GameSpeedChanged;
    }

    public abstract void ShowCharacterUI();

    public abstract void HideCharacterUI();

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public void GameSpeedChanged()
    {
        spriteAnim.SetFloat("TimeScale", BattleSystem.instance.CurrentGameSpeedTime);
    }

    public void PlayAttackAnimation()
    {
        GlobalEvents.onCharacterStartAttack?.Invoke(this);
        QuickTimeBase.onExecuteQuickTime += ExecuteAttack;
        spriteAnim.Play("Attack Windup");
    }

    public void ExecuteAttack(DamageStruct damage)
    {
        DamageStruct finalDamage = CalculateAttackDamage(damage);
        BattleSystem.instance.AttackTarget(finalDamage);
        QuickTimeBase.onExecuteQuickTime -= ExecuteAttack;
        SceneTweener.instance.ReturnToPosition(transform);
        BattleSystem.instance.EndTurn();
        GlobalEvents.onCharacterExecuteAttack?.Invoke(this, finalDamage);
        spriteAnim.Play("Attack Execute");
    }

    public virtual DamageStruct CalculateAttackDamage(DamageStruct damageStruct)
    {
        var playerClass = characterReference.characterClass;
        var enemyClass = BattleSystem.instance.GetOpposingCharacter().characterReference.characterClass;

        float effectiveness = DamageTriangle.GetEffectiveness(playerClass, enemyClass);
        damageStruct.effectivity = DamageTriangle.EffectiveFloatToEnum(effectiveness);

        float finalCritChance = critChance + critChanceModifier;
        if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn)
            finalCritChance += Convert.ToInt16(damageStruct.quickTimeSuccess) * BattleSystem.QuickTimeCritModifier;
        damageStruct.isCritical = (UnityEngine.Random.value < finalCritChance);

        damageStruct.damage = damageStruct.damageNormalized * (attack + attackModifier) * effectiveness;
        if (damageStruct.isCritical) damageStruct.damage *= (critMultiplier + critDamageModifier);

        return damageStruct;
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
                foreach (PlayerCharacter player in BattleSystem.instance.PlayerCharacters)
                {
                    targets.Add(player);
                }
                break;
            case TargetMode.AllEnemies:
                foreach (EnemyCharacter enemy in EnemyController.instance.enemies)
                {
                    targets.Add(enemy);
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
            Instantiate(skillParticles, transform);

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

            spriteAnim.Play("Skill");

            GlobalEvents.onCharacterActivateSkill?.Invoke(this);
            foreach (var subscriber in onSkillFoundTargets) subscriber();
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

        foreach (SkillObject.EffectProperties effect in currentSkill.referenceSkill.gameEffects)
        {
            if (effect.effect.targetOverride != TargetMode.None)
            {
                switch (effect.effect.targetOverride)
                {
                    case TargetMode.AllAllies:
                        foreach (PlayerCharacter player in BattleSystem.instance.PlayerCharacters)
                        {
                            ApplyEffectToCharacter(effect, player);
                        }
                        break;
                    case TargetMode.AllEnemies:
                        foreach (EnemyCharacter enemy in EnemyController.instance.enemies)
                        {
                            ApplyEffectToCharacter(effect, enemy);
                        }
                        break;
                    case TargetMode.Self:
                        ApplyEffectToCharacter(effect, this);
                        break;
                }
            }
            else
            {
                foreach (BaseCharacter character in targets)
                {
                    ApplyEffectToCharacter(effect, character);
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

        foreach (var subscriber in onFinishApplyingSkillEffects) subscriber();
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
        foreach (GameSkill skill in gameSkills)
        {
            skill.Tick();
        }

        foreach (AppliedEffect effect in AppliedEffects)
        {
            if (!effect.Tick(this))
            {
                // Queue removal of effect
                effectsToRemove.Add(effect);
            }
        }

        foreach (AppliedEffect effect in effectsToRemove)
        {
            // Remove the effect
            AppliedEffects.Remove(effect);
            onRemoveGameEffect?.Invoke(effect.referenceEffect);
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
        spriteAnim.Play("Hurt");

        onTakeDamage?.Invoke();
        GlobalEvents.onCharacterAttacked?.Invoke(this);

        if (health == 0)
        {
            if (!damage.isCritical)
            {
                PlayDamageShakeEffect(damage.damageNormalized);
                spriteAnim.Play("Death");
                Die();
            }
            else
            {
                DieToCrit();
            }
        }
        else
        {
            PlayDamageShakeEffect(damage.damageNormalized);
        }
    }

    public void PlayDamageShakeEffect(float normalizedDamage) => transform.DOShakePosition(0.75f, Mathf.Lerp(0.025f, 0.25f, normalizedDamage), 30, 90, false, true);

    public abstract void InvokeDeathEvents();
    public abstract void Die();
    public abstract void DieToCrit();

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
