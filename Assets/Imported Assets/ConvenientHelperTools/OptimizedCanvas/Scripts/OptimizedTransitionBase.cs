using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OptimizedTransitionBase : MonoBehaviour
{
    [SerializeField] protected bool ignoreTimescale = false;

    public abstract Coroutine TransitionIn();
    public abstract Coroutine TransitionOut();

    public abstract void EditorTransitionIn();
    public abstract void EditorTransitionOut();
}