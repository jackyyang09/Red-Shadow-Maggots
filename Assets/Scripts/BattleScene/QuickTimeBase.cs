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

    [SerializeField]
    protected OptimizedCanvas canvas;

    protected PlayerCharacter activePlayer = null;

    public static System.Action onExecuteQuickTime;

    public abstract void InitializeBar(BaseCharacter attacker, List<BaseCharacter> targets = null);

    public abstract void StartTicking();

    public abstract void GetMultiplier();

    [SerializeField] protected float hideDelay = 0.5f;

    public static bool AlwaysSucceed = false;

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