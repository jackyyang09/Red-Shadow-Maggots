using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class CanteenSystem : BasicSingleton<CanteenSystem>
{
    [SerializeField] float chargePerCanteen = 0.25f;
    public float ChargePerCanteen { get { return chargePerCanteen; } }
    [SerializeField] int maxCanteens = 12;

    float storedCharge;
    public float StoredCharge { get { return storedCharge; } }
    public float AvailableCharge { get { return storedCharge - grabbedCharge - borrowedCharge; } }

    float grabbedCharge;
    public float GrabbedCharge { get { return grabbedCharge; } }
    float borrowedCharge;
    public float BorrowedCharge { get { return borrowedCharge; } }

    public static Action OnAvailableChargeChanged;
    public static Action OnStoredChargeChanged;
    public static Action OnChargeBorrowed;
    public static Action OnSetCharge;

    private void Start()
    {
        AddHacks();
    }

    private void OnEnable()
    {
        BaseCharacter.OnCharacterCritChanceReduced += AddExpiredCrits;

        BattleSystem.OnEndPhase[BattlePhases.EnemyTurn.ToInt()] += ResetBonusFlag;
        BattleSystem.OnEndPhase[BattlePhases.PlayerTurn.ToInt()] += AddQTECritBonus;
    }

    private void OnDisable()
    {
        BaseCharacter.OnCharacterCritChanceReduced -= AddExpiredCrits;

        BattleSystem.OnEndPhase[BattlePhases.EnemyTurn.ToInt()] -= ResetBonusFlag;
        BattleSystem.OnEndPhase[BattlePhases.PlayerTurn.ToInt()] -= AddQTECritBonus;
    }

    public void SetCanteenCharge(float charge)
    {
        storedCharge = charge;
        OnSetCharge?.Invoke();
    }

    public void GrabCritCharge()
    {
        grabbedCharge += chargePerCanteen;
        OnAvailableChargeChanged?.Invoke();
    }

    public void ReleaseCritCharge()
    {
        grabbedCharge = 0;
        OnAvailableChargeChanged?.Invoke();
    }

    public void BorrowCritCharge(PlayerCharacter character)
    {
        character.ApplyCanteenEffect(grabbedCharge);
        borrowedCharge += grabbedCharge;
        grabbedCharge = 0;
        OnChargeBorrowed?.Invoke();
    }

    private void ResetBonusFlag()
    {
        // Change so PlayerCharacter crit numbers update properly
        BaseCharacter.IncomingDamage = new DamageStruct();
        OnStoredChargeChanged?.Invoke();
    }

    private void AddQTECritBonus()
    {
        if (BaseCharacter.IncomingDamage.qteResult == QuickTimeBase.QTEResult.Perfect)
        {
            storedCharge += BattleSystem.QuickTimeCritModifier;
        }
        else if (BaseCharacter.IncomingDamage.source)
        {
            switch (BaseCharacter.IncomingDamage.source.Reference.characterClass)
            {
                case CharacterClass.Offense:
                    storedCharge += BattleSystem.QuickTimeCritModifier * 0.5f;
                    break;
                case CharacterClass.Defense:
                    storedCharge += BattleSystem.QuickTimeCritModifier * 0.75f;
                    break;
                case CharacterClass.Support:
                    storedCharge += BattleSystem.QuickTimeCritModifier;
                    break;
            }
        }
        ResetBonusFlag();
    }

    private void AddExpiredCrits(BaseCharacter character, float changedAmount)
    {
        if (changedAmount > 0)
        {
            switch (character.Reference.characterClass)
            {
                case CharacterClass.Offense:
                    storedCharge += changedAmount * 0.5f;
                    break;
                case CharacterClass.Defense:
                    storedCharge += changedAmount * 0.75f;
                    break;
                case CharacterClass.Support:
                    storedCharge += changedAmount;
                    break;
            }
            OnStoredChargeChanged?.Invoke();
        }
    }

    #region Debug Hacks
    void AddHacks()
    {
        devConsole.AddCommand(new SickDev.CommandSystem.ActionCommand(MaxCanteenCharge)
        {
            alias = nameof(MaxCanteenCharge),
            description = "Set player characters crit chance to 100%"
        });
    }

    public void MaxCanteenCharge()
    {
        Instance.storedCharge = Instance.chargePerCanteen * Instance.maxCanteens;
        OnStoredChargeChanged?.Invoke();
        FindObjectOfType<CanteenUI>().PostHackUpdate();
        Debug.Log("Canteens maxed!");
    }
    #endregion
}