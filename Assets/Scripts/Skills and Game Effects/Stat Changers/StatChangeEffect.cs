using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat Change Effect", menuName = "ScriptableObjects/Game Effects/Stat Change Effect", order = 1)]
public class StatChangeEffect : BaseGameEffect
{
    [SerializeField] public BaseGameStat[] stats;
    public BaseGameStat stat;

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count == 0)
        {
            var value = effect.valueGroup.Values[0].GetValue(effect.targetProps);
            stat.SetGameStat(effect.Target, value);
            effect.cachedValues.Add(value);
        }
        else
        {
            var amount = effect.cachedValues[0];
            stats[0].SetGameStat(effect.Target, amount);
        }

        return base.Activate(effect);
    }

    public override void OnExpire(AppliedEffect effect)
    {
        stat.SetGameStat(effect.Target, -effect.cachedValues[0]);

        base.OnExpire(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        if (!stat) return "No effect";

        string d = "";
        float value = effect.cachedValues[0];

        if (value >= 0)
        {
            d += "Increase ";
        }
        else
        {
            d += "Decrease ";
        }

        d += stat.Name;

        d += " by ";

        switch (effect.values[0].deltaType)
        {
            case ValueType.Percentage:
                {
                    var abs = Mathf.Abs(value);
                    d += abs.FormatPercentage();
                }
                break;
            case ValueType.Value:
                var val = Mathf.FloorToInt(value);
                d += Mathf.Abs(val);
                break;
            case ValueType.Decimal:
                {
                    var abs = Mathf.Abs(value);
                    d += abs.FormatToDecimal();
                }
                break;
        }

        d = d.Substring(0, 1).ToUpper() + d.Substring(1);

        return d;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        var d = base.GetSkillDescription(targetMode, props);
        d = d.Replace("$STAT", stat.Name);

        return d + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);

        string description = TargetModeDescriptor(targetMode);
    
        description += stat.Name;

        var value = props.effectValues[0];

        if (value.multiplier >= 0 && value.flat >= 0)
        {
            description += " increased ";
        }
        else
        {
            description += " decreased ";
        }

        description += "by " + EffectValueDescriptor(value);
        
        description += DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    
        return description;
    }
}