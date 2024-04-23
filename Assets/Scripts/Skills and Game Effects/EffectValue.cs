using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseEffectValue
{
    public float FlatValue;
    public ValueType ValueType;

    public virtual float GetValue(TargetProps targetProps)
    {
        return FlatValue;
    }

    public virtual string GetSkillDescription()
    {
        switch (ValueType)
        {
            case ValueType.Percentage:
                return FlatValue.FormatPercentage();
            case ValueType.Value:
                return FlatValue.ToString();
            case ValueType.Decimal:
                return FlatValue.FormatToDecimal();
        }

        return "";
    }

    public virtual string GetEffectDescription(TargetProps targetProps)
    {
        var value = GetValue(targetProps);

        switch (ValueType)
        {
            case ValueType.Percentage:
                return value.FormatPercentage();
            case ValueType.Value:
                return value.ToString();
            case ValueType.Decimal:
                return value.FormatToDecimal();
        }

        return "";
    }
}

public class StatScaledValue : BaseEffectValue
{
    public enum EffectTarget
    {
        Caster,
        TargetCharacter
    }

    public float Multiplier;
    public EffectTarget StatSource;
    public BaseGameStat Stat;

    public override float GetValue(TargetProps targetProps)
    {
        BaseCharacter target = null;
        switch (StatSource)
        {
            case EffectTarget.Caster:
                target = targetProps.Caster;
                break;
            case EffectTarget.TargetCharacter:
                target = targetProps.Targets[0];
                break;
        }

        var v = Stat.GetGameStat(target);
        return v + base.GetValue(targetProps);
    }

    public override string GetSkillDescription()
    {
        var statName = Stat ? Stat.Name : "NO STAT";
        string d = Multiplier.FormatPercentage() + " of your " + statName;

        if (FlatValue > 0)
        {
            d += " plus " + FlatValue;
        }

        return d;
    }

    public override string GetEffectDescription(TargetProps targetProps)
    {
        var value = GetValue(targetProps);

        switch (ValueType)
        {
            case ValueType.Percentage:
                return value.FormatPercentage();
            case ValueType.Value:
                return value.ToString();
            case ValueType.Decimal:
                return value.FormatToDecimal();
        }

        return base.GetEffectDescription(targetProps);
    }
}

public class StackCountValue : StatScaledValue
{
    public BaseGameEffect StackEffect;

    public override float GetValue(TargetProps targetProps)
    {
        return base.GetValue(targetProps);
    }

    public override string GetSkillDescription()
    {
        string d = base.GetSkillDescription();

        d += " for every stack of ";

        return d;
    }
}