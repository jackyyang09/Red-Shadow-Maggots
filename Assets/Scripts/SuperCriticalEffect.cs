using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SuperCriticalEffect : MonoBehaviour
{
    [SerializeField] UnityEvent onSuperCriticalStart;
    [SerializeField] UnityEvent onSuperCriticalEnd;

    public void InvokeSuperCritStart() => onSuperCriticalStart.Invoke();
    public void InvokeSuperCritEnd() => onSuperCriticalStart.Invoke();
}