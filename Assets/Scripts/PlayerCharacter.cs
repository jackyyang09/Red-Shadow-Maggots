using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PlayerCharacter : BaseCharacter
{
    [SerializeField]
    SpriteRenderer selectionCircle;

    public static Action<PlayerCharacter> onSelectPlayer;
    public static Action<PlayerCharacter> onSelectedPlayerCharacterChange;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
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
            card.DOLocalJump(card.localPosition, 0.5f, 1, 0.25f).SetUpdate(UpdateType.Late);
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
        if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn && UIManager.CanSelect)
        {
            GlobalEvents.onSelectCharacter?.Invoke(this);
            onSelectPlayer?.Invoke(this);
            if (UIManager.SelectingAllyForSkill) return;
            if (BattleSystem.instance.GetActivePlayer() == this) return;
            BattleSystem.instance.SetActivePlayer(this);
            onSelectedPlayerCharacterChange?.Invoke(this);
        }
    }

    const float fingerHoldTime = 0.75f;
    float fingerHoldTimer = 0;

    private void OnMouseDrag()
    {
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

    public override void ShowCharacterUI()
    {
        bool isSelected = BattleSystem.instance.GetActivePlayer() == this;
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
        BattleSystem.instance.RegisterPlayerDeath(this);
        Invoke("DeathEffects", 0.5f);
    }

    public void DeathEffects()
    {
        onDeath?.Invoke();
        BattleSystem.instance.RegisterPlayerDeath(this);
        GlobalEvents.onCharacterDeath?.Invoke(this);
        GlobalEvents.onAnyPlayerDeath?.Invoke();
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        anim.SetTrigger("Death");
        //Destroy(gameObject);
    }

    public override void HideSelectionPointer()
    {
        base.HideSelectionPointer();
    }
}