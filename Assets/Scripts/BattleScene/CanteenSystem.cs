using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class CanteenSystem : BasicSingleton<CanteenSystem>
{
    [SerializeField] int canteensPerCrit = 1;
    public float ChargePerCanteen => canteensPerCrit;
    [SerializeField] int maxCanteens = 5;

    public int AvailableCanteens => canteens - BorrowedCanteens;

    public int Canteens => canteens;
    int canteens;
    int borrowedCanteens;
    public int BorrowedCanteens => borrowedCanteens;

    public static Action OnAvailableChargeChanged;
    public static Action OnSetCharge;

    private void OnEnable()
    {
        BattleSystem.OnEndPhase[BattlePhases.PlayerTurn.ToInt()] += ConfirmCanteenUse;
        BaseCharacter.OnCharacterDealDamage += OnCharacterDealDamage;
    }

    private void OnDisable()
    {
        BattleSystem.OnEndPhase[BattlePhases.PlayerTurn.ToInt()] -= ConfirmCanteenUse;
        BaseCharacter.OnCharacterDealDamage -= OnCharacterDealDamage;
    }

    void OnCharacterDealDamage(BaseCharacter a, BaseCharacter b)
    {
        if (battleSystem.CurrentPhase == BattlePhases.PlayerTurn)
        {
            if (BaseCharacter.IncomingDamage.IsCritical)
            {
                canteens++;
                OnAvailableChargeChanged?.Invoke();
            }
        }    
    }

    public void AddCanteenCharge(int c = 1)
    {
        canteens = Mathf.Min(canteens + c, maxCanteens);
        OnAvailableChargeChanged?.Invoke();
    }

    public void SetCanteenCharge(int charge)
    {
        canteens = charge;
        OnSetCharge?.Invoke();
    }

    public void ReleaseCritCharge()
    {
        borrowedCanteens = 0;
        OnAvailableChargeChanged?.Invoke();
    }

    public void BorrowCritCharge()
    {
        battleSystem.ActivePlayer.ApplyCanteenEffect();
        borrowedCanteens++;
        OnAvailableChargeChanged?.Invoke();
    }

    public void CancelCanteenUse()
    {
        battleSystem.ActivePlayer.RemoveCanteenEffects();
        borrowedCanteens = 0;
        OnAvailableChargeChanged?.Invoke();
    }

    public void ConfirmCanteenUse()
    {
        canteens -= borrowedCanteens;
        borrowedCanteens = 0;
    }

    #region Debug Hacks
    [IngameDebugConsole.ConsoleMethod(nameof(FillCritCanteens), "Fill all the player's stored canteens")]
    public static void FillCritCanteens()
    {
        Instance.canteens += Instance.maxCanteens;
        OnSetCharge?.Invoke();
        Debug.Log("Maxed canteens!");
    }
    #endregion
}