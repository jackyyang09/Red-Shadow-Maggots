using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PlayerCharacter : BaseCharacter
{
    [SerializeField]
    SpriteRenderer selectionCircle;

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
        QuickTimeBase.onExecuteQuickTime += CheckIfCrit;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        onSelectedPlayerCharacterChange -= UpdateSelectedStatus;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CheckIfCrit(DamageStruct obj)
    {
        if (BattleSystem.instance.CurrentPhase != BattlePhases.PlayerTurn) return;
        if (obj.damageType == DamageType.Heavy)
        {
            GlobalEvents.onPlayerCrit?.Invoke();
        }
    }

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
        if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn && UIManager.CanSelect)
        {
            GlobalEvents.onSelectCharacter?.Invoke(this);
            if (BattleSystem.instance.GetActivePlayer() == this) return;
            BattleSystem.instance.SetActivePlayer(this);
            onSelectedPlayerCharacterChange?.Invoke(this);
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

    public float GetAttackLeniency()
    {
        return characterReference.attackLeniency;
    }

    public float GetDefenseLeniency()
    {
        return characterReference.defenseLeniency;
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
}