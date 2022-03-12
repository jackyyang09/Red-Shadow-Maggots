using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;
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
    /// <summary>
    /// QuickTime value
    /// </summary>
    public float damageNormalized;
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
    public float AttackModifier { get { return attackModifier; } }
    public float AttackModified { get { return characterReference.attack + attackModifier; } }

    /// <summary>
    /// Additive modifier from skills
    /// </summary>
    [SerializeField]
    float defenseModifier;

    public float attackLeniencyModifier;
    public float AttackLeniency
    {
        get { return characterReference.attackLeniency + attackLeniencyModifier; }
    }

    public float defenseLeniencyModifier;
    public float DefenseLeniency
    {
        get { return characterReference.defenseLeniency + defenseLeniencyModifier; }
    }

    [SerializeField] [Range(0.02f, 1)] float critChance = 0.02f;
    /// <summary>
    /// The sum of the character's base crit chance and any modifiers before QTEs
    /// </summary>
    public virtual float CritChanceModified { get { return critChance + critChanceModifier; } }

    [SerializeField] float critChanceModifier = 0;
    public float CritChanceModifier { get { return critChanceModifier; } }

    [SerializeField] float critMultiplier = 3;
    [SerializeField] float critDamageModifier = 0;
    public float CritDamageModified { get { return critMultiplier + critDamageModifier; } }

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
    public Transform EffectRegion { get { return effectRegion; } }

    protected GameObject characterMesh;
    public GameObject CharacterMesh { get { return characterMesh; } }
    protected Animator rigAnim;

    [SerializeField] protected GameObject skillParticles1;
    [SerializeField] protected GameObject skillParticles2;

    [SerializeField] protected GameObject deathParticles;

    [SerializeField] protected SpriteRenderer selectionCircle;
    [SerializeField] protected Vector2 selectionCircleScale = new Vector2(0.7f, 1f);

    [SerializeField] AnimationHelper animHelper = null;
    public AnimationHelper AnimHelper { get { return animHelper; } }

    [SerializeField] protected GameObject canvasPrefab = null;

    public Dictionary<BaseGameEffect, List<AppliedEffect>> AppliedEffects { get; } = new Dictionary<BaseGameEffect, List<AppliedEffect>>();

    List<GameSkill> gameSkills = new List<GameSkill>();

    protected bool usedSuperCritThisTurn;

    public Action<AppliedEffect> onApplyGameEffect;
    public Action<AppliedEffect> onRemoveGameEffect;

    public Action onHeal;
    public Action onTakeDamage;

    public Action OnCharacterCritChanceChanged;

    public static Action<BaseCharacter> OnCharacterStartAttack;
    public static Action<BaseCharacter> OnCharacterAttacked;

    public static Action<BaseCharacter, GameSkill> OnCharacterActivateSkill;

    public static Action<BaseCharacter> OnCharacterDeath;

    public static Action<BaseCharacter> OnSelectCharacter;
    public static Action<BaseCharacter, DamageStruct> OnCharacterExecuteAttack;
    public static Action<BaseCharacter, float> OnCharacterCritChanceReduced;

    public static DamageStruct IncomingDamage;

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
        UIManager.OnAttackCommit += HideCharacterUI;
        BattleSystem.OnStartPlayerTurn += OnStartPlayerTurn;

        GlobalEvents.OnCharacterFinishSuperCritical += OnCharacterFinishSuperCritical;
    }

    protected virtual void OnDisable()
    {
        UIManager.OnAttackCommit -= HideCharacterUI;
        BattleSystem.OnStartPlayerTurn -= OnStartPlayerTurn;

        GlobalEvents.OnCharacterFinishSuperCritical -= OnCharacterFinishSuperCritical;
    }

    public abstract void ShowCharacterUI();

    public abstract void HideCharacterUI();

    // Update is called once per frame
    void Update()
    {
        if (selectionCircle.enabled)
        {
            float scale = Mathf.Lerp(selectionCircleScale.x, selectionCircleScale.y, Mathf.PingPong(Time.time, 1));
            selectionCircle.transform.localScale = new Vector3(scale, scale, 1);
        }
    }

    private void OnStartPlayerTurn()
    {
        usedSuperCritThisTurn = false;
        ShowCharacterUI();
        CooldownSkills();
    }

    public void UseSuperCritical()
    {
        if (rigAnim)
        {
            rigAnim.Play("Super Critical");
        }
        GlobalEvents.OnCharacterUseSuperCritical?.Invoke(this);
        usedSuperCritThisTurn = true;
        IncomingDamage.isCritical = true;
        IncomingDamage.isSuperCritical = true;
        IncomingDamage.critDamageModifier = CritDamageModified;
        animHelper.EnableCrits();
    }

    public virtual void PlayAttackAnimation()
    {
        OnCharacterStartAttack?.Invoke(this);
        QuickTimeBase.onExecuteQuickTime += ExecuteAttack;
    }

    public virtual void ExecuteAttack()
    {
        CalculateAttackDamage();

        QuickTimeBase.onExecuteQuickTime -= ExecuteAttack;

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

    public void DealDamage()
    {
        IncomingDamage.isAOE = false;
        BattleSystem.Instance.AttackTarget();
        OnCharacterExecuteAttack?.Invoke(this, IncomingDamage);
    }

    public void DealAOEDamage()
    {
        IncomingDamage.isAOE = true;
        BattleSystem.Instance.AttackAOE();
        OnCharacterExecuteAttack?.Invoke(this, IncomingDamage);
    }

    public virtual void BeginAttack(Transform target)
    {
        IncomingDamage = new DamageStruct();
        IncomingDamage.source = this;

        if (CritChanceModified >= 1 && !usedSuperCritThisTurn)
        {
            UseSuperCritical();
        }
        else
        {
            switch (Reference.range)
            {
                case AttackRange.CloseRange:
                    SceneTweener.Instance.MeleeTweenTo(transform, target);
                    break;
                case AttackRange.LongRange:
                    SceneTweener.Instance.RangedTweenTo(CharacterMesh.transform, target);
                    break;
            }

            if (Reference.attackQteType == QTEType.SimpleBar)
            {
                PlayAttackAnimation();
            }
        }
    }

    public void FinishAttack()
    {
        switch (Reference.range)
        {
            case AttackRange.CloseRange:
                SceneTweener.Instance.ReturnToPosition();
                break;
            case AttackRange.LongRange:
            case AttackRange.AOE:
                SceneTweener.Instance.RotateBack();
                break;
        }
        if (IncomingDamage.isCritical)
        {
            animHelper.DisableCrits();
        }
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
            IncomingDamage.isSuperCritical = finalCritChance >= 1.0f;
            animHelper.EnableCrits();
        }
        IncomingDamage.critDamageModifier = CritDamageModified;
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
                        EnemyCharacter.OnSelectedEnemyCharacterChange += AddSkillTarget;
                        break;
                    case BattlePhases.EnemyTurn:
                        targets.Add(battleSystem.EnemyAttackTarget);
                        break;
                }
                break;
            case TargetMode.AllAllies:
                for (int i = 0; i < BattleSystem.Instance.PlayerCharacters.Count; i++)
                {
                    targets.Add(BattleSystem.Instance.PlayerCharacters[i]);
                }
                break;
            case TargetMode.AllEnemies:
                for (int i = 0; i < EnemyController.Instance.Enemies.Count; i++)
                {
                    targets.Add(EnemyController.Instance.Enemies[i]);
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
            rigAnim.Play("Skill");
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

    public void ApplyEffectToCharacter(SkillObject.EffectProperties effectAndDuration, BaseCharacter character)
    {
        Instantiate(effectAndDuration.effect.particlePrefab, character.transform);
        effectAndDuration.effect.Activate(character, effectAndDuration.strength, effectAndDuration.customValues);
        if (effectAndDuration.effectDuration == 0) return;

        AppliedEffect newEffect = new AppliedEffect();

        newEffect.target = character;
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
                        for (int j = 0; j < BattleSystem.Instance.PlayerCharacters.Count; j++)
                        {
                            ApplyEffectToCharacter(effect, BattleSystem.Instance.PlayerCharacters[j]);
                        }
                        break;
                    case TargetMode.AllEnemies:
                        for (int j = 0; j < EnemyController.Instance.Enemies.Count; j++)
                        {
                            ApplyEffectToCharacter(effect, EnemyController.Instance.Enemies[j]);
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
        EffectTextSpawner.instance.SpawnEffectAt(newEffect.referenceEffect, transform);
        onApplyGameEffect?.Invoke(newEffect);
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
                onRemoveGameEffect?.Invoke(AppliedEffects[effect][0]);
                AppliedEffects[effect].RemoveAt(0);
            }
        }
        else if (AppliedEffects[effect].Count > 1) 
        {
            var newList = effect.TickMultiple(this, AppliedEffects[effect]);
            var diff = AppliedEffects[effect].Except(newList).ToList();

            for (int i = 0; i < diff.Count; i++)
            {
                onRemoveGameEffect?.Invoke(diff.ElementAt(i));
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

    public virtual void TakeDamage(DamageStruct damage)
    {
        var myClass = characterReference.characterClass;
        CharacterClass attackerClass = CharacterClass.Offense;
        if (damage.source != null)
        {
            attackerClass = damage.source.Reference.characterClass;
            float effectiveness = DamageTriangle.GetEffectiveness(attackerClass, myClass);
            damage.effectivity = DamageTriangle.EffectiveFloatToEnum(effectiveness);

            damage.damage = damage.damageNormalized * damage.source.AttackModified * effectiveness;
        }

        if (damage.isCritical) damage.damage *= damage.critDamageModifier;

        float trueDamage = CalculateDefenseDamage(damage.damage);
        health = Mathf.Clamp(health - trueDamage, 0, maxHealth);

        DamageNumberSpawner.instance.SpawnDamageNumberAt(transform.parent, damage);
        if (rigAnim)
        {
            if (IncomingDamage.qteResult == QuickTimeBase.QTEResult.Perfect)
            {
                if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn)
                    rigAnim.Play("Hit Reaction");
                else
                    rigAnim.Play("Block Reaction");
            }
            else
            {
                if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn)
                    rigAnim.Play("Block Reaction");
                else
                    rigAnim.Play("Hit Reaction");
            }

            if (damage.source != null && !damage.isAOE)
            {
                characterMesh.transform.LookAt(damage.source.transform);
            }
        }

        onTakeDamage?.Invoke();
        OnCharacterAttacked?.Invoke(this);

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

    public GameObject SpawnEffectPrefab(GameObject prefab, bool removeParent = false, bool destroyAutomatically = true)
    {
        effectRegion.rotation = characterMesh.transform.rotation;
        var newEffect = Instantiate(prefab, effectRegion.transform.position, effectRegion.rotation);
        //newEffect.transform.localEulerAngles = Vector3.zero;
        if (!removeParent)
        {
            newEffect.transform.SetParent(animHelper.transform.GetChild(0).GetChild(0));
        }
        if (destroyAutomatically)
        {
            Destroy(newEffect, 5);
        }
        return newEffect;
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
}