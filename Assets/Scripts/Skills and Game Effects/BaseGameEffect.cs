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

/// <summary>
/// The base definition of a singular game effect
/// 
/// Reflection: Rather than creating a zillion new ScriptableObject instances:
/// Have GameAbilityObject inherit from ScriptableObject, and have it contain a reference 
/// to a subclass of BaseGameAbility
/// </summary>
#if UNITY_EDITOR
[UnityEditor.CanEditMultipleObjects]
#endif
public abstract class BaseGameEffect : ScriptableObject
{
    public Sprite effectIcon;

    public string effectName;
    public string effectText;

    public EffectType effectType = EffectType.None;

    public bool activateOnDeath;

    public GameObject particlePrefab;
    public GameObject tickPrefab;

    public JSAM.SoundFileObject activationSound;
    public JSAM.SoundFileObject tickSound;

    [TextArea] public string skillDescription;
    [TextArea] public string effectDescription;

    [HideInInspector] public virtual bool IncludesExplainer { get; }
    /// <summary>
    /// Only used if IncludesExplainer is true
    /// </summary>
    [HideInInspector] public virtual string ExplainerName { get; }
    [HideInInspector] public virtual string ExplainerDescription { get; }

    /// <summary>
    /// The number of indices of CachedValues this effect accesses
    /// </summary>
    public virtual int ValueCount => 1;

    /// <summary>
    /// Overrides default tick time if value is above -1
    /// </summary>
    public virtual float TickAnimationTime => 0;

    /// <summary>
    /// 
    /// </summary>
    /// <returns>bool - False if Activation condition not met</returns>
    public virtual bool Activate(AppliedEffect effect) => true;

    /// <summary>
    /// Called on every turn after it's activation
    /// </summary>
    public virtual void Tick(AppliedEffect effect) { }
    
    public virtual void OnExpire(AppliedEffect effect) { }

    public virtual void OnDeath(AppliedEffect effect) { }

    public virtual void OnSpecialCallback(AppliedEffect effect) { }

    public virtual string GetSkillDescription(EffectGroup eg)
    {
        var d = skillDescription;

        var props = eg.effectProps;
        var targetMode = eg.effectTarget;

        if (d.Contains("$STACKS"))
        {
            if (props.stacks < 0)
            {
                d = d.Replace("Apply", "Remove");
            }
            d = d.Replace("$STACKS", Mathf.Abs(props.stacks).ToString());
        }

        d = d.Replace("$EFFECT", props.effect.effectName);

        if (eg.appStyle != null) d = d.Replace("$TARGET", eg.appStyle.ProcessTargetDescriptor(targetMode));

        if (props.value != null)
        {
            d = props.value.ProcessSkillDescription(targetMode, d);
        }

        return d;
    }

    public virtual string GetEffectDescription(AppliedEffect effect)
    {
        var d = effectDescription;

        return d;
    }

    public static string DurationAndActivationDescriptor(int turns, int activations)
    {
        if (activations <= 0 && turns <= 0) return "";

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
        s += ") ";
        return s;
    }
}

public interface IStackableEffect
{
    public void OnStacksChanged(AppliedEffect effect, int previous);
}