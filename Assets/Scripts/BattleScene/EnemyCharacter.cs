using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Facade;

public class EnemyCharacter : BaseCharacter
{
    [SerializeField] float chanceToUseSkill = 0.3f;
    public void SetSkillUseChance(float newChance) => chanceToUseSkill = newChance;

    public float ChanceToUseSkill
    {
        get { return chanceToUseSkill; }
    }

    bool usedSkillThisTurn;
    public bool UsedSkillThisTurn => usedSkillThisTurn;

    [SerializeField] int critLevel = 0;

    // Enemies shouldn't be able to crit randomly
    public override float CritChanceModified => 0;

    public int CritLevel => critLevel;

    public override bool CanCrit => critLevel == Reference.turnsToCrit;

    [SerializeField] bool isBossCharacter = false;

    public static System.Action<EnemyCharacter> OnSelectedEnemyCharacterChange;
    public System.Action<int> onCritLevelChanged;

    protected bool usedSuperCritThisTurn;
    public bool UsedSuperCritThisTurn => usedSuperCritThisTurn;

    public override void InitializeWithInfo(int level, BattleState.State stateInfo = null)
    {
        base.InitializeWithInfo(level, stateInfo);

        superCritSkill = new GameSkill(characterReference.superCritical);

        if (stateInfo != null)
        {
            if (health == 0)
            {
                DieSilently();
                return;
            }

            critLevel = ((BattleState.EnemyState)stateInfo).Crit;
            onCritLevelChanged?.Invoke(critLevel);
        }

        onSetHealth?.Invoke();
    }

    protected override void OnCharacterLoaded(AsyncOperationHandle<GameObject> obj)
    {
        base.OnCharacterLoaded(obj);

        characterMesh.transform.eulerAngles = new Vector3(0, 90, 0);
    }

    protected override IEnumerator CreateBillboardUI()
    {
        // Set to World-Scale of 1
        if (isBossCharacter)
        {
            //characterMesh.transform.localScale = new Vector3(0.5714285f, 0.5714285f, 0.5714285f);
            ui.InitializeBossUIWithCharacter(this);
        }
        else
        {
            billBoard = Instantiate(canvasPrefab, ui.ViewportBillboardCanvas.transform)
                .GetComponent<ViewportBillboard>();
        }

        yield return new WaitUntil(() => Initialized);

        EnableBillboardUI();
        if (!isBossCharacter)
        {
            billBoard.GetComponent<CharacterUI>().InitializeWithCharacter(this);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        OnSelectedEnemyCharacterChange += UpdateSelectedStatus;
        BattleSystem.OnEndPhase[BattlePhases.EnemyTurn.ToInt()] += OnEndPhase;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        OnSelectedEnemyCharacterChange -= UpdateSelectedStatus;
        BattleSystem.OnEndPhase[BattlePhases.EnemyTurn.ToInt()] -= OnEndPhase;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    void OnEndPhase()
    {
        IncreaseChargeLevel();

        usedSuperCritThisTurn = false;
        usedSkillThisTurn = false;
    }

    public void IncreaseChargeLevel(int i = 1)
    {
        if (!usedSkillThisTurn || (usedSkillThisTurn && critLevel != Reference.turnsToCrit - 1))
        {
            critLevel = (int)Mathf.Clamp(critLevel + i, 0, Reference.turnsToCrit);
            onCritLevelChanged?.Invoke(critLevel);
        }

        if (usedSuperCritThisTurn) ResetChargeLevel();
    }

    public void DecreaseChargeLevel(int i = 1)
    {
        if (!usedSkillThisTurn || (usedSkillThisTurn && critLevel != Reference.turnsToCrit - 1))
        {
            critLevel = (int)Mathf.Clamp(critLevel - i, 0, Reference.turnsToCrit);
            onCritLevelChanged?.Invoke(critLevel);
        }

        usedSkillThisTurn = false;
    }

    public void ResetChargeLevel()
    {
        critLevel = 0;
        onCritLevelChanged?.Invoke(critLevel);
        usedSkillThisTurn = false;
    }

    public override void BeginAttack(BaseCharacter target)
    {
        if (!CanCrit)
        {
            battleStateManager.InitializeRandom();

            int attackIndex = Random.Range(0, Reference.enemyAttackAnimations.Length);
            var attack = Reference.enemyAttackAnimations[attackIndex];
            IncomingAttack = attack;
            PlayAttackAnimation(attackIndex);
        }

        base.BeginAttack(target);
    }

    public override void UseSuperCritical()
    {
        base.UseSuperCritical();

        usedSuperCritThisTurn = true;

        var a = new AttackStruct();
        a.attackAnimation = Reference.superCriticalAnim;
        a.attackRange = AttackRange.LongRange;
        IncomingAttack = a;
    }

    public void PlayAttackAnimation()
    {
        OnCharacterStartAttack?.Invoke(this);
        QuickTimeBase.OnExecuteAnyQuickTime += ExecuteAttack;

        if (rigAnim)
        {
            if (Reference.attackAnimations.Length > 1)
            {
                rigAnim.Play("Attack Level 2");
            }
            else
            {
                rigAnim.Play("Enemy Attack Level 2");
            }
        }
        else
        {
            spriteAnim.Play("Attack Windup");
        }
    }

    public void PlayAttackAnimation(int selectedAttack)
    {
        OnCharacterStartAttack?.Invoke(this);
        QuickTimeBase.OnExecuteAnyQuickTime += ExecuteAttack;

        rigAnim.Play("Enemy Attack Level " + (selectedAttack + 1).ToString());
    }

    public void UpdateSelectedStatus(EnemyCharacter selectedEnemy)
    {
        if (selectedEnemy == this)
        {
            ShowCharacterUI();
        }
        else
        {
            HideCharacterUI();
        }
    }

    public override void ShowCharacterUI()
    {
        bool isSelected = battleSystem.ActiveEnemy == this;
        selectionCircle.enabled = isSelected;
    }

    private void OnMouseDown()
    {
        if (battleSystem.CurrentPhase == BattlePhases.PlayerTurn && UIManager.CanSelectCharacter)
        {
            battleSystem.TrySetActiveEnemy(this);
        }
    }

    public override void UseSkill(GameSkill skill)
    {
        usedSkillThisTurn = true;
        base.UseSkill(skill);
    }

    protected override void HandleCrits()
    {
        if (CanCrit)
        {
            IncomingDamage.IsSuperCritical = true;
            IncomingDamage.IsCritical = true;
        }

        base.HandleCrits();
    }

    public override void ExecuteAttack()
    {
        base.ExecuteAttack();

        if (rigAnim)
        {
            rigAnim.SetTrigger("Attack Execute");
            rigAnim.SetInteger("Charge Level", 4);
        }
        else
        {
            spriteAnim.Play("Attack Execute");
        }
    }

    public override float CalculateAttack(DamageStruct damage, float effectiveness)
    {
        return AttackModified * damage.Percentage * effectiveness * damage.QTEEnemy;
    }

    public override float CalculateDefense(DamageStruct damage)
    {
        return (1 - damage.QTEEnemy * DefenseModified) * (1 + damageAbsorptionModifier);
    }

    public override void Die()
    {
        base.Die();
        Invoke(nameof(DeathEffects), 0.5f);
    }

    public override void DieSilently()
    {
        base.DieSilently();
        if (isBossCharacter) ui.DestroyBossUI();
    }

    public override void InvokeDeathEvents()
    {
        base.InvokeDeathEvents();
        onDeath?.Invoke();
        EnemyController.Instance.RegisterEnemyDeath(this);
        GlobalEvents.OnAnyEnemyDeath?.Invoke();
        OnCharacterDeath?.Invoke(this);
    }

    public void DeathEffects()
    {
        InvokeDeathEvents();

        Instantiate(deathParticles, transform.position, Quaternion.identity);
        //anim.SetTrigger("Death");
    }

    public void ForceHideSelectionPointer()
    {
        HideSelectionPointer();
    }
}

public static class EnemyCharacterExtension
{
    public static bool IsEnemy(this BaseCharacter b, out EnemyCharacter enemy)
    {
        enemy = b as EnemyCharacter;
        return enemy;
    }
}