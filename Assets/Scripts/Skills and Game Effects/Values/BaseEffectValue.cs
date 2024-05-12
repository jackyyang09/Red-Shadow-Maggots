using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ValueOperator
{
    Addition,
    Multiplication
}

[System.Serializable]
public class BaseEffectValue
{
    public float FlatValue;
    public ValueType ValueType;
    public ValueOperator Operator = ValueOperator.Addition;

    public virtual float GetValue(TargetProps targetProps)
    {
        return FlatValue;
    }

    public virtual string ProcessSkillDescription(string description, int index)
    {
        string key = "$FLAT" + index;

        if (description.Contains(key))
        {
            description = description.Replace(key, FlatValue.FormatTo(ValueType));
        }

        return description;
    }

    public virtual string GetEffectDescription(TargetProps targetProps)
    {
        var value = GetValue(targetProps);

        return value.FormatTo(ValueType);
    }
}