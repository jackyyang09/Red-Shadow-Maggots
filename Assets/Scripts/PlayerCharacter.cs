using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using static Facade;

public class PlayerCharacter : BaseCharacter
{
    [SerializeField] Transform cardMesh = null;

    const float fingerHoldTime = 0.75f;
    float fingerHoldTimer = 0;

    float canteenCharge;

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

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        characterMesh.transform.eulerAngles = new Vector3(0, -90, 0);

        var billboard = Instantiate(canvasPrefab, ui.ViewportBillboardCanvas.transform).GetComponent<ViewportBillboard>();
        billboard.EnableWithSettings(sceneTweener.SceneCamera, CharacterMesh.transform);
        billboard.GetComponent<CharacterUI>().InitializeWithCharacter(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        OnSelectedPlayerCharacterChange += UpdateSelectedStatus;
        BattleSystem.OnStartPlayerTurn += RemoveCanteenEffects;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OnSelectedPlayerCharacterChange -= UpdateSelectedStatus;
        BattleSystem.OnEndPlayerTurn -= RemoveCanteenEffects;
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
        if (isSelected)
        {
            cardMesh.DOLocalJump(cardMesh.localPosition, 0.5f, 1, 0.25f).SetUpdate(UpdateType.Late);
        }
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
        if (!UIManager.CanSelectPlayer) return;
        if (!ui.CharacterPanelOpen && !UIManager.SelectingAllyForSkill)
        {
            fingerHoldTimer += Time.deltaTime;
            if (fingerHoldTimer >= fingerHoldTime)
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
                    }
                    else
                    {
                        rigAnim.SetInteger("Charge Level", IncomingDamage.chargeLevel);
                    }
                    if (IncomingDamage.isSuperCritical) rigAnim.SetInteger("Charge Level", 4);
                    rigAnim.SetTrigger("Attack Execute");
                    break;
            }
        }
        else
        {
            spriteAnim.Play("Attack Execute");
        }
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
        Invoke("DeathEffects", 0.5f);
    }

    public override void InvokeDeathEvents()
    {
        BattleSystem.Instance.RegisterPlayerDeath(this);
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