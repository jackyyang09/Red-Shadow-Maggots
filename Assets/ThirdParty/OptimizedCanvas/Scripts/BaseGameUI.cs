using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameUI : MonoBehaviour
{
    [SerializeField] protected OptimizedCanvas optimizedCanvas;
    public OptimizedCanvas OptimizedCanvas { get { return optimizedCanvas; } }

    public abstract void ShowUI();
    public abstract void HideUI();
}