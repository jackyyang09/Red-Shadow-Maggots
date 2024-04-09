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

    public virtual string GetSkillDescription(TargetMode targetMode, EffectProperties props) => "";

    public virtual string GetEffectDescription(AppliedEffect effect) => "";

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
        s += ")";
        return s;
    }

    public static string EffectValueDescriptor(EffectProperties.EffectValue value, BaseGameStat stat = null)
    {
        return EffectValueDescriptor(value, "your", stat);
    }

    public static string EffectValueDescriptor(EffectProperties.EffectValue value, string subject, BaseGameStat stat = null)
    {
        string d = "";
        if (value.multiplier != 0)
        {
            d = Mathf.Abs(value.multiplier).FormatPercentage() + " ";
        }
        if (stat)
        {
            d += "of " + subject + " " + stat.Name + " ";
        }
        if (value.multiplier != 0 && value.flat != 0)
        {
            d += "plus ";
        }
        if (value.flat != 0)
        {
            var abs = Mathf.Abs(value.flat);
            switch (value.flatType)
            {
                case EffectProperties.EffectType.Percentage:
                    d += abs.FormatPercentage();
                    break;
                case EffectProperties.EffectType.Value:
                    d += abs;
                    break;
                case EffectProperties.EffectType.Decimal:
                    d += abs.FormatToDecimal();
                    break;
            }
            d += " ";
        }
        return d;
    }

    public static float GetValue(BaseGameStat stat, EffectProperties.EffectValue value, BaseCharacter source)
    {
        float o = 0;
        if (value.multiplier != 0)
        {
            o = stat.GetGameStat(source) * value.multiplier;
        }
        return o + value.flat;
    }
}

public interface IStackableEffect
{
    public void OnStacksChanged(AppliedEffect effect);
}