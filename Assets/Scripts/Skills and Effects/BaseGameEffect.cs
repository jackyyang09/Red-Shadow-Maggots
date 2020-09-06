using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameEffect : ScriptableObject
{
    /// <summary>
    /// Turns the effect is active for, 0 for instantaneous
    /// </summary>
    public int effectDuration;

    public GameObject particlePrefab;

    /// <summary>
    /// Invoked immediately
    /// </summary>
    /// <param name="targets"></param>
    public abstract void Activate(List<BaseCharacter> targets);

    /// <summary>
    /// Called on every turn after it's activation
    /// </summary>
    public abstract void Tick();
}