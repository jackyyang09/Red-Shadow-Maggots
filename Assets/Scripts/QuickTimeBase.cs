using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuickTimeBase : MonoBehaviour
{
    [SerializeField]
    protected OptimizedCanvas canvas;

    public static System.Action<DamageStruct> onExecuteQuickTime;

    public abstract void InitializeBar(float leniency);

    public abstract void StartTicking();

    public abstract DamageStruct GetMultiplier();

    [SerializeField]
    protected float hideDelay = 0.5f;

    public void Hide()
    {
        canvas.Hide();
    }

    public void Enable()
    {
        enabled = true;
    }
}