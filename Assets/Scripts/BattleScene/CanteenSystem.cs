using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class CanteenSystem : BasicSingleton<CanteenSystem>
{
    [SerializeField] float chargePerCanteen = 0.25f;
    public float ChargePerCanteen => chargePerCanteen;
    [SerializeField] int maxCanteens = 12;

    public float StoredCharge { get { return storedCharge; } }
    public float AvailableCharge { get { return storedCharge - grabbedCharge - borrowedCharge; } }
    float storedCharge;

    float grabbedCharge;
    public float GrabbedCharge { get { return grabbedCharge; } }
    float borrowedCharge;
    public float BorrowedCharge { get { return borrowedCharge; } }

    public static Action OnAvailableChargeChanged;
    public static Action OnStoredChargeChanged;
    public static Action OnChargeBorrowed;
    public static Action OnSetCharge;

    private void OnEnable()
    {
        BaseCharacter.OnCharacterCritChanceReduced += AddExpiredCrits;

        BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] += ResetBonusFlag;
        BattleSystem.OnEndPhase[BattlePhases.PlayerTurn.ToInt()] += AddQTECritBonus;
    }

    private void OnDisable()
    {
        BaseCharacter.OnCharacterCritChanceReduced -= AddExpiredCrits;

        BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] -= ResetBonusFlag;
        BattleSystem.OnEndPhase[BattlePhases.PlayerTurn.ToInt()] -= AddQTECritBonus;
    }

    public void AddCanteenCharge(float charge)
    {
        storedCharge = Mathf.Clamp(storedCharge + charge, 0, maxCanteens * ChargePerCanteen);
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
        if (BaseCharacter.IncomingDamage.QTEResult == QuickTimeBase.QTEResult.Perfect)
        {
            AddCanteenCharge(BattleSystem.QuickTimeCritModifier);
        }
        else if (BaseCharacter.IncomingDamage.Source)
        {
            switch (BaseCharacter.IncomingDamage.Source.Reference.characterClass)
            {
                case CharacterClass.Offense:
                    AddCanteenCharge(BattleSystem.QuickTimeCritModifier * 0.5f);
                    break;
                case CharacterClass.Defense:
                    AddCanteenCharge(BattleSystem.QuickTimeCritModifier * 0.75f);
                    break;
                case CharacterClass.Support:
                    AddCanteenCharge(BattleSystem.QuickTimeCritModifier);
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
                    AddCanteenCharge(changedAmount * 0.5f);
                    break;
                case CharacterClass.Defense:
                    AddCanteenCharge(changedAmount * 0.75f);
                    break;
                case CharacterClass.Support:
                    AddCanteenCharge(changedAmount);
                    break;
            }
            OnStoredChargeChanged?.Invoke();
        }
    }

    #region Debug Hacks
    [IngameDebugConsole.ConsoleMethod(nameof(FillCritCanteens), "Fill all the player's stored canteens")]
    public static void FillCritCanteens()
    {
        Instance.storedCharge = Instance.chargePerCanteen * Instance.maxCanteens;
        OnStoredChargeChanged?.Invoke();
        FindObjectOfType<CanteenUI>().PostHackUpdate();
        Debug.Log("Maxed stored charge!");
    }
    #endregion
}