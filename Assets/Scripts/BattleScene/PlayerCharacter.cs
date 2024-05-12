using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Facade;

public class PlayerCharacter : BaseCharacter
{
    const float SPECIAL_ATTACK_DELAY = 0.35f;

    int canteensBorrowed;

    public override bool CanCrit => CritChanceModified >= 1;
    public override bool CanUseSuperCrit => superCritSkill.EffectiveCooldown?.Invoke() <= 0;

    public Action OnSetActivePlayer;
    public static Action<PlayerCharacter> OnSelectPlayer;
    public static Action<PlayerCharacter> OnSelectedPlayerCharacterChange;
    public static Action<PlayerCharacter> OnPlayerQTEAttack;

    public override void InitializeWithInfo(int level, BattleState.State stateInfo = null)
    {
        base.InitializeWithInfo(level, stateInfo);

        maxHealth = characterReference.GetMaxHealth(currentLevel, false)/* * RarityMultiplier*/;

        superCritSkill = new GameSkill(characterReference.superCritical, 
            () => Mathf.Max(superCritSkill.CooldownTimer - canteensBorrowed, 0));

        if (stateInfo != null)
        {
            var s = stateInfo as BattleState.PlayerState;
            if (s.Cooldowns != null)
            {
                for (int i = 0; i < s.Cooldowns.Length; i++)
                {
                    gameSkills[i].CooldownTimer = s.Cooldowns[i];
                }
            }
        }
        else
        {
            health = maxHealth;
        }

        onSetHealth?.Invoke();
    }

    protected override void OnCharacterLoaded(AsyncOperationHandle<GameObject> obj)
    {
        base.OnCharacterLoaded(obj);

        characterMesh.transform.eulerAngles = new Vector3(0, -90, 0);

        waitEntity.IsPlayerControlled = true;
        waitEntity.MovesOnPlayerTurn = true;

        battleSystem.AddPlayerToWaitList(this, waitEntity);
    }

    protected override IEnumerator CreateBillboardUI()
    {
        billBoard = Instantiate(canvasPrefab, ui.ViewportBillboardCanvas.transform).GetComponent<ViewportBillboard>();

        yield return new WaitUntil(() => Initialized);

        EnableBillboardUI();

        billBoard.GetComponent<CharacterUI>().InitializeWithCharacter(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        OnSelectedPlayerCharacterChange += UpdateSelectedStatus;
        BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] += RemoveCanteenEffects;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OnSelectedPlayerCharacterChange -= UpdateSelectedStatus;
        BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] -= RemoveCanteenEffects;
    }

    // Update is called once per frame
    //void Update()
    //{
    //
    //}

    public void UpdateSelectedStatus(PlayerCharacter newSelection)
    {
        bool isSelected = newSelection == this;
    }

    private void OnMouseDown()
    {
        if (UIManager.SelectingAllyForSkill)
        {
            OnSelectPlayer?.Invoke(this);
            return;
        }
    }

    public virtual void InitiateDefense()
    {
        if (battleSystem.CurrentPhase == BattlePhases.EnemyTurn)
        {
            QuickTimeBase.OnExecuteAnyQuickTime += PlayBlockAnimation;
        }
    }

    public virtual void EndDefense()
    {
        QuickTimeBase.OnExecuteAnyQuickTime -= PlayBlockAnimation;
    }

    void PlayBlockAnimation(DamageStruct d) => PlayBlockAnimation();
    void PlayBlockAnimation()
    {
        if (rigAnim)
        {
            rigAnim.Play("Block Initiate");
        }
    }

    public override void BeginAttack(BaseCharacter target)
    {
        IncomingAttack = Reference.attackAnimations[0];

        OnPlayerQTEAttack?.Invoke(this);

        if (Reference.attackQteType == QTEType.SimpleBar)
        {
            Windup();
        }

        base.BeginAttack(target);
    }

    public void Windup()
    {
        OnCharacterStartAttack?.Invoke(this);
        QuickTimeBase.OnExecuteAnyQuickTime += ExecuteAttack;

        if (rigAnim)
        {
            rigAnim.Play("Attack Windup");
        }
        else
        {
            spriteAnim.Play("Attack Windup");
        }
    }

    protected override void HandleCrits()
    {
        float finalCritChance = CritChanceModified;

        bool qteSuccess = IncomingDamage.QTEResult == QuickTimeBase.QTEResult.Perfect;

        OnCharacterCritChanceChanged?.Invoke();
        IncomingDamage.IsCritical = UnityEngine.Random.value < finalCritChance;
        if (IncomingDamage.IsCritical)
        {
            IncomingDamage.IsSuperCritical = finalCritChance >= 1.0f;
        }

        if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn)
        {
            OnExecuteAttack?.Invoke(qteSuccess);
        }
        else
        {
            OnExecuteAttack?.Invoke(!qteSuccess);
        }

        base.HandleCrits();
    }

    public override void ExecuteAttack()
    {
        base.ExecuteAttack();

        if (rigAnim)
        {
            switch (Reference.attackQteType)
            {
                case QTEType.SimpleBar:
                    if (IncomingDamage.IsSuperCritical) rigAnim.SetInteger("Charge Level", 4);
                    if (EnhancedBasicAttack)
                    {
                        rigAnim.SetInteger("Charge Level", 0);
                        rigAnim.SetTrigger("Attack Execute");
                    }
                    else
                    {
                        rigAnim.Play("Attack Execute");
                    }
                    break;
                case QTEType.Hold:
                    if (IncomingDamage.QTEResult != QuickTimeBase.QTEResult.Perfect)
                    {
                        rigAnim.SetInteger("Charge Level", 0);
                        rigAnim.SetTrigger("Attack Execute");
                    }
                    else
                    {
                        Invoke(nameof(ExecuteDelayedAttack), SPECIAL_ATTACK_DELAY);
                    }
                    if (IncomingDamage.IsSuperCritical)
                    {
                        rigAnim.SetInteger("Charge Level", 4);
                        rigAnim.SetTrigger("Attack Execute");
                    }
                    break;
            }
        }
        else
        {
            spriteAnim.Play("Attack Execute");
        }
    }

    public void ExecuteDelayedAttack()
    {
        rigAnim.SetInteger("Charge Level", IncomingDamage.ChargeLevel);
        rigAnim.SetTrigger("Attack Execute");
    }

    public override float CalculateAttack(DamageStruct damage, float effectiveness)
    {
        return AttackModified * damage.Percentage * effectiveness * damage.QTEPlayer;
    }

    public override float CalculateDefense(DamageStruct damage)
    {
        return (1 - damage.QTEPlayer * DefenseModified) * (1 + damageAbsorptionModifier);
    }

    public void ApplyCanteenEffect()
    {
        canteensBorrowed += 1;
    }

    public void RemoveCanteenEffects()
    {
        canteensBorrowed = 0;
    }

    public override void ShowCharacterUI()
    {
        bool isSelected = battleSystem.ActivePlayer == this;
        selectionCircle.enabled = isSelected;
    }

    public override void Die()
    {
        base.Die();
        Invoke(nameof(DeathEffects), 0.5f);
    }

    public override void DieSilently()
    {
        base.DieSilently();
    }

    public override void InvokeDeathEvents()
    {
        base.InvokeDeathEvents();
        battleSystem.RegisterPlayerDeath(this);
    }

    public void DeathEffects()
    {
        InvokeDeathEvents();

        Instantiate(deathParticles, transform.position, Quaternion.identity);
        //anim.SetTrigger("Death");
    }
}

public static class PlayerCharacterExtension
{
    public static bool IsPlayer(this BaseCharacter b)
    {
        return b as PlayerCharacter;
    }
}