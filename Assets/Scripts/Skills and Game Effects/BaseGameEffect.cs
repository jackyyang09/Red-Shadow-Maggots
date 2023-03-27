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
#if UNITY_EDITOR
[UnityEditor.CanEditMultipleObjects]
#endif
public abstract class BaseGameEffect : ScriptableObject
{
    public Sprite effectIcon;

    public string effectText;

    public EffectType effectType = EffectType.None;

    public bool canStack = false;

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

    /// <summary>
    /// Stackable effects will implement this
    /// </summary>
    /// <param name="target"></param>
    /// <param name="customValues"></param>
    public virtual void TickCustom(BaseCharacter target, List<object> values) { }

    public List<AppliedEffect> TickMultiple(BaseCharacter target, List<AppliedEffect> effects)
    {
        var remainingEffects = new List<AppliedEffect>(effects);
        var values = new List<object>();
        for (int i = 0; i < effects.Count; i++)
        {
            values.Add(GetEffectStrength(effects[i].strength, effects[i].customValues));
            if (!effects[i].TickSilent())
            {
                remainingEffects.Remove(effects[i]);
            }
        }
        TickCustom(target, values);
        return remainingEffects;
    }

    public abstract void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues);

    public abstract object GetEffectStrength(EffectStrength strength, float[] customValues);

    public abstract string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration);

    protected string DurationDescriptor(int turns)
    {
        string s = "";
        if (turns > 0)
        {
            s += "(" + turns + " Turn";
            if (turns > 1) s += "s";
            s += ")";
        }
        return s;
    }

    protected string TargetModeDescriptor(TargetMode mode)
    {
        switch (mode)
        {
            case TargetMode.OneAlly:
                return "Ally ";
            case TargetMode.OneEnemy:
                return "Enemy ";
            case TargetMode.AllAllies:
                return "All Allies ";
            case TargetMode.AllEnemies:
                return "All Enemies ";
        }
        return "";
    }
}