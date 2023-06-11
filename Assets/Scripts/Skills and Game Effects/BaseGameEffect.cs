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

    public JSAM.SoundFileObject activationSound;
    public JSAM.SoundFileObject tickSound;

    /// <summary>
    /// Invoked immediately
    /// </summary>
    /// <param name="targets"></param>
    public virtual void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues) { }

    /// <summary>
    /// Called on every turn after it's activation
    /// </summary>
    public virtual void Tick(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues) { }

    /// <summary>
    /// Stackable effects will implement this
    /// </summary>
    /// <param name="target"></param>
    /// <param name="customValues"></param>
    public virtual void TickCustom(BaseCharacter user, BaseCharacter target, List<object> values) { }

    public List<AppliedEffect> TickMultiple(BaseCharacter user, BaseCharacter target, List<AppliedEffect> effects)
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
        TickCustom(user, target, values);
        return remainingEffects;
    }

    public virtual void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues) { }

    public virtual object GetEffectStrength(EffectStrength strength, float[] customValues) { return null; }

    public virtual string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration) { return ""; }

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