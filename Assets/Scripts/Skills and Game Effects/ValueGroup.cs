using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ValueGroup
{
    [SerializeReference] public BaseEffectValue[] Values = new BaseEffectValue[] { new BaseEffectValue() };

    public float GetValue(TargetProps targetProps)
    {
        if (Values.Length == 0) return 0;

        float o = Values[0].GetValue(targetProps);

        for (int i = 1; i < Values.Length; i++)
        {
            switch (Values[i].Operator)
            {
                case ValueOperator.Addition:
                    o += Values[i].GetValue(targetProps);
                    break;
                case ValueOperator.Multiplication:
                    o *= Values[i].GetValue(targetProps);
                    break;
            }
        }
        return o;
    }

    public ValueGroup ShallowCopy()
    {
        return MemberwiseClone() as ValueGroup;
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