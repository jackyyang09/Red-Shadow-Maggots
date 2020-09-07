using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public struct DamageStruct
{
    public float damage;
    public DamageType damageType;
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

    [SerializeField]
    float maxHealth;

    [SerializeField]
    float attack;

    [SerializeField]
    AttackRange range;

    [Header("Object References")]

    [SerializeField]
    SpriteRenderer sprite;

    [SerializeField]
    protected Transform card;

    [SerializeField]
    protected GameObject deathParticles;

    List<BaseGameEffect> appliedEffects;

    List<GameSkill> gameSkills;

    protected Animator anim;

    public Action onHeal;
    public Action onTakeDamage;

    public UnityEngine.Events.UnityEvent onDeath;

    [ContextMenu("Apply Reference")]
    public void ApplyReferenceProperties()
    {
        if (characterReference == null) return;
        maxHealth = characterReference.maxHealth;
        attack = characterReference.attack;
        sprite.sprite = characterReference.sprite;
        range = characterReference.range;

        health = maxHealth;
    }

    public void SetReference(CharacterObject newRef)
    {
        characterReference = newRef;
    }

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = maxHealth;

        foreach (SkillObject skillObject in characterReference.skills)
        {
            GameSkill newSkill = new GameSkill();
            newSkill.InitWithSkill(skillObject);
        }
    }

    protected virtual void OnEnable()
    {
        UIManager.onAttackCommit += HideCharacterUI;
        BattleSystem.onStartPlayerTurn += ShowCharacterUI;
        BattleSystem.onStartPlayerTurn += TickSkills;
    }

    protected virtual void OnDisable()
    {
        UIManager.onAttackCommit -= HideCharacterUI;
        BattleSystem.onStartPlayerTurn -= ShowCharacterUI;
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
        QuickTimeBase.onExecuteQuickTime += ExecuteAttack;
    }

    public void ExecuteAttack(DamageStruct damage)
    {
        BattleSystem.instance.AttackTarget(CalculateAttackDamage(damage));
        QuickTimeBase.onExecuteQuickTime -= ExecuteAttack;
        SceneTweener.instance.ReturnToPosition(transform);
        BattleSystem.instance.EndTurn();
    }

    public virtual DamageStruct CalculateAttackDamage(DamageStruct damageStruct)
    {
        damageStruct.damage *= attack;
        return damageStruct;
    }

    public virtual float CalculateDefenseDamage(float damage)
    {
        return damage;
    }

    public void UseSkill(int index)
    {
        currentSkill = gameSkills[index];
        StartCoroutine(SkillRoutine());

        GlobalEvents.onSelectCharacter += AddTarget;
    }

    List<BaseCharacter> targets;

    GameSkill currentSkill;

    IEnumerator SkillRoutine()
    {
        targets = new List<BaseCharacter>();

        GlobalEvents.onSelectCharacter += (x) => targets.Add(x);

        if (currentSkill.referenceSkill.targetMode == TargetMode.None)
        {
            targets.Add(this);
        }

        while (targets == null)
        {
            yield return null;
        }

        GlobalEvents.onSelectCharacter -= (x) => targets.Add(x);

        currentSkill.Activate(targets);
    }


    public void AddTarget(BaseCharacter newTarget)
    {
        switch (skill.referenceSkill.targetMode)
        {
            case TargetMode.OneAlly:
                break;
            case TargetMode.OneEnemy:
                break;
            case TargetMode.AllAllies:
                break;
            case TargetMode.AllEnemies:
                break;
        }
    }

    private void TickSkills()
    {
        foreach (GameSkill skill in gameSkills)
        {
            skill.Tick();
        }
    }

    public virtual void Heal(float healthGain)
    {
        health = Mathf.Clamp(health + healthGain, 0, maxHealth);
        onHeal?.Invoke();
    }

    public virtual void TakeDamage(DamageStruct damage)
    {
        float trueDamage = CalculateDefenseDamage(damage.damage);
        health = Mathf.Clamp(health - trueDamage, 0, maxHealth);
        DamageNumberSpawner.instance.SpawnDamageNumberAt(damage.damage, transform.position, damage.damageType);

        onTakeDamage?.Invoke();
        transform.DOShakePosition(0.75f, 0.25f, 30, 90, false, true);
        if (health == 0)
        {
            Die();
        }
    }

    public abstract void Die();

    public float GetHealthPercent()
    {
        return health / maxHealth;
    }
}
