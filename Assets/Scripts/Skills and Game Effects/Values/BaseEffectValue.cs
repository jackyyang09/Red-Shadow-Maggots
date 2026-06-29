using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class BaseEffectValue
{
    public ValueType ValueType;

    public abstract float GetValue(TargetProps targetProps);

    public abstract string ProcessSkillDescription(BaseEffectTarget target, string description);

    public abstract string Descriptor { get; }

    public abstract BattleState.SerializedValue Serialize();

    public virtual string GetEffectDescription(AppliedEffect effect)
    {
        //var value = GetValue(effect.targetProps);
        return effect.cachedValues[0].Value.FormatTo(effect.cachedValues[0].Type);
    }

    /// <summary>
    /// Shallow-Copy
    /// </summary>
    /// <returns></returns>
    public virtual BaseEffectValue Clone()
    {
        return MemberwiseClone() as BaseEffectValue;
    }
}

[System.Serializable]
public class FlatValue : BaseEffectValue
{
    public float Flat;

    public override float GetValue(TargetProps targetProps)
    {
        return Flat;
    }

    public override string Descriptor => Flat.FormatTo(ValueType);

    public override string ProcessSkillDescription(BaseEffectTarget target, string description)
    {
        description = description.Replace("$VALUE", Descriptor);

        return description;
    }

    public override BattleState.SerializedValue Serialize()
    {
        return new()
        { 
            Type = nameof(FlatValue),
            Values = new[] { Flat.ToString(), ((int)ValueType).ToString() }
        };
    }
}

public static class ValueExtensions
{
    public static string FormatTo(this float value, ValueType valueType)
    {
        string valueString = "";
        switch (valueType)
        {
            case ValueType.Percentage:
                {
                    var abs = Mathf.Abs(value);
                    valueString = abs.FormatPercentage();
                }
                break;
            case ValueType.Value:
                var val = Mathf.FloorToInt(value);
                valueString = Mathf.Abs(val).ToString();
                break;
            case ValueType.Decimal:
                {
                    var abs = Mathf.Abs(value);
                    valueString = abs.FormatToDecimal();
                }
                break;
        }
        return valueString;
    }
}