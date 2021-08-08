using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PlayerCharacter : BaseCharacter
{
    [SerializeField] SpriteRenderer selectionCircle = null;

    [SerializeField] Transform cardMesh = null;

    const float fingerHoldTime = 0.75f;
    float fingerHoldTimer = 0;

    public static Action<PlayerCharacter> onSelectPlayer;
    public static Action<PlayerCharacter> onSelectedPlayerCharacterChange;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        characterMesh.transform.eulerAngles = new Vector3(0, -90, 0);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        onSelectedPlayerCharacterChange += UpdateSelectedStatus;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        onSelectedPlayerCharacterChange -= UpdateSelectedStatus;
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
        anim.SetBool("Selected", isSelected);
        if (isSelected)
        {
            cardMesh.DOLocalJump(cardMesh.localPosition, 0.5f, 1, 0.25f).SetUpdate(UpdateType.Late);
        }
    }

    public void ForceSelect()
    {
        selectionCircle.enabled = true;
        anim.SetBool("Selected", true);
    }

    public void ForceDeselct()
    {
        selectionCircle.enabled = false;
        anim.SetBool("Selected", false);
    }

    private void OnMouseDown()
    {
        if (UIManager.instance.CharacterPanelOpen) return;
        if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn && UIManager.CanSelectPlayer)
        {
            GlobalEvents.OnSelectCharacter?.Invoke(this);
            onSelectPlayer?.Invoke(this);
            if (BattleSystem.Instance.GetActivePlayer() == this) return;
            if (UIManager.SelectingAllyForSkill) return;
            if (IsDead) return;
            BattleSystem.Instance.SetActivePlayer(this);
            onSelectedPlayerCharacterChange?.Invoke(this);
        }
    }

    private void OnMouseDrag()
    {
        if (!UIManager.CanSelectPlayer) return;
        if (!UIManager.instance.CharacterPanelOpen && !UIManager.SelectingAllyForSkill)
        {
            fingerHoldTimer += Time.deltaTime;
            if (fingerHoldTimer >= fingerHoldTime)
            {
                UIManager.instance.OpenCharacterPanel();
                fingerHoldTimer = 0;
            }
        }
    }

    private void OnMouseUp()
    {
        fingerHoldTimer = 0;
    }

    public override void ShowCharacterUI()
    {
        bool isSelected = BattleSystem.Instance.GetActivePlayer() == this;
        selectionCircle.enabled = isSelected;
        anim.SetBool("Selected", isSelected);
    }

    public override void HideCharacterUI()
    {
        selectionCircle.enabled = false;
        anim.SetBool("Selected", false);
    }

    public float attackLeniencyModifier;
    public float GetAttackLeniency()
    {
        return characterReference.attackLeniency + attackLeniencyModifier;
    }

    public float defenceLeniencyModifier;
    public float GetDefenceLeniency()
    {
        return characterReference.defenceLeniency + defenceLeniencyModifier;
    }

    public override void Die()
    {
        Invoke("DeathEffects", 0.5f);
    }

    public override void InvokeDeathEvents()
    {
        BattleSystem.Instance.RegisterPlayerDeath(this);
        onDeath?.Invoke();
        GlobalEvents.OnCharacterDeath?.Invoke(this);
        GlobalEvents.OnAnyPlayerDeath?.Invoke();
    }

    public void DeathEffects()
    {
        InvokeDeathEvents();

        Instantiate(deathParticles, transform.position, Quaternion.identity);
        anim.SetTrigger("Death");
    }

    public override void HideSelectionPointer()
    {
        base.HideSelectionPointer();
    }
}