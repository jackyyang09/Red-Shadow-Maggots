using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : BaseCharacter
{
    [SerializeField] bool isBossCharacter = false;

    public static System.Action<EnemyCharacter> onSelectedEnemyCharacterChange;

    protected override void Start()
    {
        base.Start();

        // Set to World-Scale of 1
        if (isBossCharacter)
        {
            characterMesh.transform.localScale = new Vector3(0.5714285f, 0.5714285f, 0.5714285f);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        onSelectedEnemyCharacterChange += UpdateSelectedStatus;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        onSelectedEnemyCharacterChange -= UpdateSelectedStatus;
    }


    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

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
        bool isSelected = BattleSystem.instance.GetActiveEnemy() == this;
        selectionPointer.enabled = isSelected;
        anim.SetBool("Selected", isSelected);
    }

    public override void HideCharacterUI()
    {
        selectionPointer.enabled = false;
        anim.SetBool("Selected", false);
    }

    private void OnMouseDown()
    {
        if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn && UIManager.CanSelectPlayer)
        {
            if (IsDead) return;
            GlobalEvents.OnSelectCharacter?.Invoke(this);
            if (BattleSystem.instance.GetActiveEnemy() == this) return;
            BattleSystem.instance.SetActiveEnemy(this);
            onSelectedEnemyCharacterChange?.Invoke(this);
        }
    }

    public override void ExecuteAttack(DamageStruct damage)
    {
        CalculateAttackDamage(damage);

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
        EnemyController.instance.RegisterEnemyDeath(this);
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
        if (BattleSystem.instance.GetActiveEnemy() != this)
        {
            base.HideSelectionPointer();
        }
    }
}