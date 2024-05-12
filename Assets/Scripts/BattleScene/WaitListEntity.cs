using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitListEntity
{
    public delegate float WaitDelegate();
    readonly WaitDelegate getWait;
    readonly WaitDelegate getWaitLimit;

    public float WaitTimer;
    public float Wait => getWait();
    public float WaitPercentage => WaitTimer / getWaitLimit();
    public float WaitLimit => getWaitLimit();
    public bool IsOverWait => WaitPercentage >= 1;

    public bool IsPlayerControlled;
    public bool MovesOnPlayerTurn;

    public BaseCharacter Character;

    public IEnumerator EffectRoutine;

    public Sprite Headshot;

    public System.Action Move;
    public System.Action OnStartTurn;
    public System.Action OnEndTurn;

    public System.Action OnWaitChanged;
    public System.Action OnWaitTimerChanged;
    public System.Action OnWaitLimitChanged;

    public WaitListEntity(WaitDelegate getWait, WaitDelegate getWaitLimit)
    {
        this.getWait = getWait;
        this.getWaitLimit = getWaitLimit;
    }

    /// <summary>
    /// Called by BattleSystem
    /// </summary>
    public void IncrementWaitTimer()
    {
        WaitTimer += Wait;
        OnWaitTimerChanged?.Invoke();
    }

    /// <summary>
    /// Called by BattleSystem
    /// </summary>
    public void ResetWait()
    {
        WaitTimer = 0;
    }

    public void RemoveSelf()
    {
        if (Character) Character.OnDeath -= RemoveSelf;
        BattleSystem.Instance.RemoveEntityFromWaitlist(this);
    }
}

[CreateAssetMenu(fileName = "New Waitist Object", menuName = "ScriptableObjects/Wait List Object", order = 1)]
public class WaitListObject : ScriptableObject
{
    public string Name;
    public Sprite HeadshotSprite;
    public float Wait;
    public float WaitLimit;
}