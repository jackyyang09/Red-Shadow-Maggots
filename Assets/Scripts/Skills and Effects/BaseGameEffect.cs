using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    None,
    Heal,
    Buff,
    Debuff,
    Damage
}

public enum EffectStrength
{
    Custom,
    Weak,
    Small,
    Medium,
    Large,
    EX
}

/// <summary>
/// The base definition of a singular game effect
/// </summary>
[UnityEditor.CanEditMultipleObjects]
public abstract class BaseGameEffect : ScriptableObject
{
    public Sprite effectIcon;

    public string effectText;

    public TargetMode targetOverride = TargetMode.None;

    public EffectType effectType = EffectType.None;

    public bool hasTickAnimation = false;

    public GameObject particlePrefab;

    public JSAM.JSAMSoundFileObject activationSound;
    public JSAM.JSAMSoundFileObject tickSound;

    /// <summary>
    /// Invoked immediately
    /// </summary>
    /// <param name="targets"></param>
    public abstract void Activate(BaseCharacter target, EffectStrength strength, float[] customValues);

    /// <summary>
    /// Called on every turn after it's activation
    /// </summary>
    public abstract void Tick(BaseCharacter target, EffectStrength strength, float[] customValues);

    public abstract void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues);

    public abstract string GetEffectDescription(EffectStrength strength, float[] customValues);
}