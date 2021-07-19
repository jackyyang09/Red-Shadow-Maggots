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

    public static System.Action<DamageStruct> onExecuteQuickTime;

    public abstract void InitializeBar(PlayerCharacter player);

    public abstract void StartTicking();

    public abstract DamageStruct GetMultiplier();

    [SerializeField]
    protected float hideDelay = 0.5f;

    public System.Action<DamageStruct> OnQuickTimeComplete;

    protected void OnEnable()
    {
        GlobalEvents.OnEnterBattleCutscene += Hide;
    }

    protected void OnDisable()
    {
        GlobalEvents.OnEnterBattleCutscene -= Hide;
    }

    public void Hide()
    {
        if (IsInvoking("Hide")) CancelInvoke("Hide");
        canvas.Hide();
        activePlayer = null;
        enabled = false;
    }

    public void Enable()
    {
        enabled = true;
    }
}