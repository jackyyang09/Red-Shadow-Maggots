using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using static Facade;

public class PlayerCharacter : BaseCharacter
{
    [SerializeField] float currentExperience = 0;
    [SerializeField] float maxExperience = 100;

    const float FINGER_HOLD_TIME = 0.75f;
    float fingerHoldTimer = 0;

    float canteenCharge;

    const float SPECIAL_ATTACK_DELAY = 0.35f;

    public override float CritChanceModified
    {
        get
        { 
            return base.CritChanceModified + canteenCharge +
                System.Convert.ToInt32(IncomingDamage.qteResult == QuickTimeBase.QTEResult.Perfect) * BattleSystem.QuickTimeCritModifier;
        }
    }

    public static Action<PlayerCharacter> OnSelectPlayer;
    public static Action<PlayerCharacter> OnSelectedPlayerCharacterChange;
    public static Action<PlayerCharacter> OnPlayerQTEAttack;

    public override void ApplyCharacterStats(int level, BattleState.State stateInfo = null)
    {
        base.ApplyCharacterStats(level, stateInfo);

        maxHealth = characterReference.GetMaxHealth(currentLevel, false)/* * RarityMultiplier*/;

        if (stateInfo != null)
        {
            var s = stateInfo as BattleState.PlayerState;
            if (s.Cooldowns != null)
            {
                for (int i = 0; i < s.Cooldowns.Length; i++)
                {
                    gameSkills[i].cooldownTimer = s.Cooldowns[i];
                }
            }
        }
        else
        {
            health = maxHealth;
        }
    }

    protected override void Initialize()
    {
        base.Initialize();

        characterMesh.transform.eulerAngles = new Vector3(0, -90, 0);

        billBoard = Instantiate(canvasPrefab, ui.ViewportBillboardCanvas.transform).GetComponent<ViewportBillboard>();
        billBoard.EnableWithSettings(sceneTweener.SceneCamera, CharacterMesh.transform);
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
        selectionCircle.enabled = isSelected;
    }

    public void ForceSelect()
    {
        selectionCircle.enabled = true;
    }

    public void ForceDeselect()
    {
        selectionCircle.enabled = false;
    }

    private void OnMouseDown()
    {
        if (playerControlManager.CurrentMode >= PlayerControlMode.InCutscene) return;
        if (ui.CharacterPanelOpen) return;
        if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn && UIManager.CanSelectPlayer)
        {
            OnSelectCharacter?.Invoke(this);
            OnSelectPlayer?.Invoke(this);
            if (battleSystem.ActivePlayer == this) return;
            if (UIManager.SelectingAllyForSkill) return;
            if (IsDead) return;
            BattleSystem.Instance.SetActivePlayer(this);
            OnSelectedPlayerCharacterChange?.Invoke(this);
        }
    }

    private void OnMouseDrag()
    {
        if (playerControlManager.CurrentMode >= PlayerControlMode.InCutscene) return;
        if (!UIManager.CanSelectPlayer) return;
        if (!ui.CharacterPanelOpen && !UIManager.SelectingAllyForSkill)
        {
            fingerHoldTimer += Time.deltaTime;
            if (fingerHoldTimer >= FINGER_HOLD_TIME)
            {
                ui.OpenCharacterPanel();
                fingerHoldTimer = 0;
            }
        }
    }

    private void OnMouseUp()
    {
        fingerHoldTimer = 0;
    }

    public virtual void InitiateDefense()
    {
        if (battleSystem.CurrentPhase == BattlePhases.EnemyTurn)
        {
            QuickTimeBase.onExecuteQuickTime += PlayBlockAnimation;
        }
    }

    public virtual void EndDefense()
    {
        QuickTimeBase.onExecuteQuickTime -= PlayBlockAnimation;
    }

    void PlayBlockAnimation(DamageStruct d) => PlayBlockAnimation();
    void PlayBlockAnimation()
    {
        if (rigAnim)
        {
            rigAnim.Play("Block Initiate");
        }
    }

    public override void BeginAttack(Transform target)
    {
        IncomingAttack = Reference.attackAnimations[0];

        if (CritChanceModified < 1 || usedSuperCritThisTurn)
        {
            OnPlayerQTEAttack?.Invoke(this);
        }

        if (Reference.attackQteType == QTEType.SimpleBar)
        {
            PlayAttackAnimation();
        }

        base.BeginAttack(target);
    }

    public override void PlayAttackAnimation()
    {
        base.PlayAttackAnimation();

        if (rigAnim)
        {
            rigAnim.Play("Attack Windup");
        }
        else
        {
            spriteAnim.Play("Attack Windup");
        }
    }

    public override void ExecuteAttack()
    {
        base.ExecuteAttack();

        if (rigAnim)
        {
            switch (Reference.attackQteType)
            {
                case QTEType.SimpleBar:
                    if (IncomingDamage.isSuperCritical) rigAnim.SetInteger("Charge Level", 4);
                    rigAnim.Play("Attack Execute");
                    break;
                case QTEType.Hold:
                    if (IncomingDamage.qteResult != QuickTimeBase.QTEResult.Perfect)
                    {
                        rigAnim.SetInteger("Charge Level", 0);
                        rigAnim.SetTrigger("Attack Execute");
                    }
                    else
                    {
                        Invoke(nameof(ExecuteDelayedAttack), SPECIAL_ATTACK_DELAY);
                    }
                    if (IncomingDamage.isSuperCritical)
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
        rigAnim.SetInteger("Charge Level", IncomingDamage.chargeLevel);
        rigAnim.SetTrigger("Attack Execute");
    }

    public void ApplyCanteenEffect(float critCharge)
    {
        canteenCharge += critCharge;
        OnCharacterCritChanceChanged?.Invoke();
    }

    public void RemoveCanteenEffects()
    {
        canteenCharge = 0;
        OnCharacterCritChanceChanged?.Invoke();
    }

    public override void ShowCharacterUI()
    {
        bool isSelected = battleSystem.ActivePlayer == this;
        selectionCircle.enabled = isSelected;
    }

    public override void HideCharacterUI()
    {
        selectionCircle.enabled = false;
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
        onDeath?.Invoke();
        OnCharacterDeath?.Invoke(this);
        GlobalEvents.OnAnyPlayerDeath?.Invoke();
    }

    public void DeathEffects()
    {
        InvokeDeathEvents();

        Instantiate(deathParticles, transform.position, Quaternion.identity);
        //anim.SetTrigger("Death");
    }

    public override void HideSelectionPointer()
    {
        base.HideSelectionPointer();
    }
}