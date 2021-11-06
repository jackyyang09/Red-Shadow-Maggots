using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGameUI : MonoBehaviour
{
    [SerializeField] protected OptimizedCanvas optimizedCanvas;
    public OptimizedCanvas OptimizedCanvas { get { return optimizedCanvas; } }
}