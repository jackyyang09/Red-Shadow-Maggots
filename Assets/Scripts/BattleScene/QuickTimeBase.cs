using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public abstract class QuickTimeBase : MonoBehaviour
{
    public enum QTEResult
    {
        Early,
        Perfect,
        Late
    }

    [SerializeField] protected float maxLeniency = 2;
    [SerializeField] protected float baseLeniency = 1;

    [SerializeField] protected OptimizedCanvas canvas;

    protected PlayerCharacter activePlayer;

    public abstract void InitializeBar(BaseCharacter attacker);

    public abstract void StartTicking();

    public abstract void GetMultiplier();

    [SerializeField] protected float hideDelay = 0.5f;

    protected float totalLeniency;

    public static bool AlwaysSucceed = false;

    public static System.Action OnExecuteAnyQuickTime;
    public System.Action OnExecuteQuickTime;
    public System.Action OnQuickTimeComplete;

    protected virtual void Start()
    {
        //GlobalEvents.OnEnterBattleCutscene += Hide;
    }

    protected virtual void OnDestroy()
    {
        //GlobalEvents.OnEnterBattleCutscene -= Hide;
    }

    public void Hide()
    {
        if (IsInvoking(nameof(Hide))) CancelInvoke(nameof(Hide));
        canvas.Hide();
        activePlayer = null;
        enabled = false;
    }

    public void Enable()
    {
        enabled = true;
    }

    [IngameDebugConsole.ConsoleMethod(nameof(LockQTESuccess), "Quick-time events will always succeed")]
    public static void LockQTESuccess()
    {
        AlwaysSucceed = true;
    }
}