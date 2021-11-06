using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class EnemyCharacter : BaseCharacter
{
    [SerializeField] int critLevel = 0;
    public bool CanCrit { get { return critLevel == Reference.turnsToCrit; } }
    [SerializeField] bool isBossCharacter = false;

    public static Action<EnemyCharacter> OnSelectedEnemyCharacterChange;
    public Action<int> onCritLevelChanged;

    protected override void Start()
    {
        base.Start();

        // Set to World-Scale of 1
        if (isBossCharacter)
        {
            //characterMesh.transform.localScale = new Vector3(0.5714285f, 0.5714285f, 0.5714285f);
            ui.InitializeBossUIWithCharacter(this);
        }
        else
        {
            var billboard = Instantiate(canvasPrefab, ui.ViewportBillboardCanvas.transform).GetComponent<ViewportBillboard>();
            billboard.EnableWithSettings(sceneTweener.SceneCamera, CharacterMesh.transform);
            billboard.GetComponent<CharacterUI>().InitializeWithCharacter(this);
        }
        
        characterMesh.transform.eulerAngles = new Vector3(0, 90, 0);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        OnSelectedEnemyCharacterChange += UpdateSelectedStatus;
    }

    public void IncreaseChargeLevel()
    {
        critLevel = (int)Mathf.Repeat(critLevel + 1, Reference.turnsToCrit + 1);
        onCritLevelChanged?.Invoke(critLevel);
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

    public override void PlayAttackAnimation()
    {
        base.PlayAttackAnimation();

        if (rigAnim)
        {
            rigAnim.Play("Attack Execute");
        }
        else
        {
            spriteAnim.Play("Attack Windup");
        }
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
        if (!isBossCharacter)
        {
            selectionPointer.enabled = isSelected;
            anim.SetBool("Selected", isSelected);
        }
    }

    public override void HideCharacterUI()
    {
        if (!isBossCharacter)
        {
            selectionPointer.enabled = false;
            anim.SetBool("Selected", false);
        }
    }

    private void OnMouseDown()
    {
        if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn && UIManager.CanSelectPlayer)
        {
            if (IsDead) return;
            GlobalEvents.OnSelectCharacter?.Invoke(this);
            if (battleSystem.ActiveEnemy == this) return;
            BattleSystem.Instance.SetActiveEnemy(this);
            OnSelectedEnemyCharacterChange?.Invoke(this);
        }
    }

    public override void ExecuteAttack(DamageStruct damage)
    {
        CalculateAttackDamage(damage);
        if (CanCrit)
        {
            incomingDamage.isSuperCritical = true;
            incomingDamage.isCritical = true;
        }

        QuickTimeBase.onExecuteQuickTime -= ExecuteAttack;

        if (rigAnim)
        {
            rigAnim.SetTrigger("Attack Execute");
            rigAnim.SetInteger("Charge Level", 0);
        }
        else
        {
            spriteAnim.Play("Attack Execute");
        }
    }

    public override void Die()
    {
        Invoke("DeathEffects", 0.5f);
    }

    public override void InvokeDeathEvents()
    {
        onDeath?.Invoke();
        EnemyController.Instance.RegisterEnemyDeath(this);
        GlobalEvents.OnAnyEnemyDeath?.Invoke();
        GlobalEvents.OnCharacterDeath?.Invoke(this);
    }

    public void DeathEffects()
    {
        InvokeDeathEvents();

        Instantiate(deathParticles, transform.position, Quaternion.identity);
        anim.SetTrigger("Death");
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