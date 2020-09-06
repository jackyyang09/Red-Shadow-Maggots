using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCharacter : BaseCharacter
{
    [SerializeField]
    SpriteRenderer selectionCircle;

    public static System.Action<PlayerCharacter> onSelectedPlayerCharacterChange;

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
    void Update()
    {
        
    }

    public void UpdateSelectedStatus(PlayerCharacter newSelection)
    {
        bool isSelected = newSelection == this;
        selectionCircle.enabled = isSelected;
        anim.SetBool("Selected", isSelected);
        if (isSelected)
        {
            card.DOLocalJump(card.localPosition, 0.5f, 1, 0.25f);
        }
    }

    private void OnMouseDown()
    {
        if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn && UIManager.CanSelect)
        {
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
        GlobalEvents.onPlayerDeath?.Invoke();
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        anim.SetTrigger("Death");
        //Destroy(gameObject);
    }
}