using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [SerializeField] int critLevel = 0;

    // Enemies shouldn't be able to crit randomly
    public override float CritChanceModified => 0;

    public int CritLevel
    {
        get { return critLevel; }
    }

    public override bool CanCrit
    {
        get { return critLevel == Reference.turnsToCrit; }
    }

    [SerializeField] bool isBossCharacter = false;

    public static System.Action<EnemyCharacter> OnSelectedEnemyCharacterChange;
    public System.Action<int> onCritLevelChanged;

    public override void ApplyCharacterStats(int level, BattleState.State stateInfo = null)
    {
        base.ApplyCharacterStats(level, stateInfo);

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
    }

    protected override void Initialize()
    {
        base.Initialize();

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
            billBoard.EnableWithSettings(sceneTweener.SceneCamera, CharacterMesh.transform);
            billBoard.GetComponent<CharacterUI>().InitializeWithCharacter(this);
        }

        characterMesh.transform.eulerAngles = new Vector3(0, 90, 0);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        OnSelectedEnemyCharacterChange += UpdateSelectedStatus;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        OnSelectedEnemyCharacterChange -= UpdateSelectedStatus;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public void IncreaseChargeLevel(int i = 1)
    {
        if (!usedSkillThisTurn || (usedSkillThisTurn && critLevel != Reference.turnsToCrit - 1))
        {
            critLevel = (int)Mathf.Clamp(critLevel + i, 0, Reference.turnsToCrit);
            onCritLevelChanged?.Invoke(critLevel);
        }

        usedSkillThisTurn = false;
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

    public override void BeginAttack(Transform target)
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

        var a = new AttackStruct();
        a.attackAnimation = Reference.superCriticalAnim;
        a.attackRange = AttackRange.LongRange;
        IncomingAttack = a;
    }

    public override void PlayAttackAnimation()
    {
        base.PlayAttackAnimation();

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
        base.PlayAttackAnimation();

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

    public override void HideCharacterUI()
    {
        selectionCircle.enabled = false;
    }

    private void OnMouseDown()
    {
        if (battleSystem.CurrentPhase == BattlePhases.PlayerTurn && UIManager.CanSelectPlayer)
        {
            if (IsDead) return;
            OnSelectCharacter?.Invoke(this);
            if (battleSystem.ActiveEnemy == this) return;
            battleSystem.SetActiveEnemy(this);
            OnSelectedEnemyCharacterChange?.Invoke(this);
        }
    }

    public override void UseSkill(GameSkill skill)
    {
        usedSkillThisTurn = true;
        base.UseSkill(skill);
    }

    public override void ExecuteAttack()
    {
        CalculateAttackDamage();

        if (CanCrit)
        {
            IncomingDamage.isSuperCritical = true;
            IncomingDamage.isCritical = true;
        }

        QuickTimeBase.onExecuteQuickTime -= ExecuteAttack;

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
        base.HideSelectionPointer();
    }

    public override void HideSelectionPointer()
    {
        if (battleSystem.ActiveEnemy != this)
        {
            base.HideSelectionPointer();
        }
    }
}