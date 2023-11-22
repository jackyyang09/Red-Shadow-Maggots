using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Policy;
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

    [HideInInspector] public virtual bool IncludesExplainer { get; }
    /// <summary>
    /// Only used if IncludesExplainer is true
    /// </summary>
    [HideInInspector] public virtual string ExplainerName { get; }
    [HideInInspector] public virtual string ExplainerDescription { get; }

    public EffectType effectType = EffectType.None;

    public bool activateOnApply = true;
    public bool activateOnDeath;

    public bool canStack;

    /// <summary>
    /// Overrides default tick time if value is above -1
    /// </summary>
    public virtual float TickAnimationTime => 0;

    public GameObject particlePrefab;
    public GameObject tickPrefab;

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

    public virtual void OnDeath(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues) { }

    public virtual object GetEffectStrength(EffectStrength strength, float[] customValues) { return null; }

    public virtual string GetSkillDescription(TargetMode targetMode, EffectProperties props) => "";

    public virtual string GetEffectDescription(EffectStrength strength, float[] customValues) => "";

    protected string DurationAndActivationDescriptor(int turns, int activations)
    {
        string s = "(";
        if (activations > 0)
        {
            s += activations + " Time";
            if (activations > 1) s += "s";
        }
        if (turns > 0)
        {
            if (activations > 0) s += ", ";
            s += turns + " Turn";
            if (turns > 1) s += "s";
        }
        s += ")";
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

/// <summary>
/// An instance of a GameEffect to be attached to an instanced Character
/// </summary>
public class AppliedEffect
{
    public BaseCharacter caster;
    public BaseCharacter target;
    public BaseGameEffect referenceEffect;
    public int remainingTurns;
    public int remainingActivations;
    public EffectStrength strength;
    public float[] customValues;
    public string description;
    public float cachedValue;

    public void Apply()
    {
        if (referenceEffect.activateOnApply)
        {
            Activate();
        }
    }

    public bool Activate()
    {
        referenceEffect.Activate(caster, target, strength, customValues);
        if (remainingActivations > 0)
        {
            remainingActivations--;
            if (remainingActivations == 0) return false;
        }
        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="target"></param>
    /// <returns>Is effect still active?</returns>
    public bool Tick()
    {
        remainingTurns--;
        referenceEffect.Tick(caster, target, strength, customValues);
        if (remainingTurns == 0)
        {
            referenceEffect.OnExpire(caster, target, strength, customValues);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Unlike Tick, doesn't activate the effect when called
    /// </summary>
    /// <returns>Is effect still active?</returns>
    public bool TickSilent()
    {
        remainingTurns--;
        if (remainingTurns == 0)
        {
            referenceEffect.OnExpire(caster, target, strength, customValues);
            return false;
        }
        return true;
    }

    public void OnDeath()
    {
        referenceEffect.OnDeath(caster, target, strength, customValues);
        if (referenceEffect.activateOnDeath)
        {
            if (remainingActivations > 0)
            {
                remainingActivations--;
                if (remainingActivations == 0)
                {
                    target.RemoveEffect(this);
                }
            }
        }
    }
}