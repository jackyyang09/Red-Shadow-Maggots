using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public System.Action OnExecuteQuickTime;
    public System.Action OnQuickTimeComplete;

    protected virtual void OnEnable()
    {
        GlobalEvents.OnEnterBattleCutscene += Hide;
    }

    protected virtual void OnDisable()
    {
        GlobalEvents.OnEnterBattleCutscene -= Hide;
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
}