using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class OptimizedTransitionBase : MonoBehaviour
{
    public System.Action OnTransitionIn;
    public System.Action OnTransitionOut;

    public UnityEvent OnTransitionInStartUnityEvent;
    public UnityEvent OnTransitionInEndUnityEvent;
    public UnityEvent OnTransitionOutStartUnityEvent;
    public UnityEvent OnTransitionOutEndUnityEvent;

    public void InvokeOnTransitionIn()
    {
        OnTransitionIn?.Invoke();
        OnTransitionInEndUnityEvent.Invoke();
    }

    public void InvokeOnTransitionOut()
    {
        OnTransitionOut?.Invoke();
        OnTransitionOutEndUnityEvent.Invoke();
    }

    [SerializeField] protected bool ignoreTimescale = false;

    public abstract float GetTransitionInTime();
    public abstract float GetTransitionOutTime();

    public abstract Coroutine TransitionIn();
    public abstract Coroutine TransitionOut();

    public abstract void EditorTransitionIn();
    public abstract void EditorTransitionOut();
}