using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiStatChangeEffect : BaseGameEffect
{
    [SerializeField] protected BaseGameStat[] stats;

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count == 0)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                var value = GetValue(stats[i], effect.values[i], effect.target);
                stats[i].SetGameStat(effect.target, value);
                effect.cachedValues.Add(value);
            }
        }
        else
        {
            for (int i = 0; i < stats.Length; i++)
            {
                var amount = effect.cachedValues[i];
                stats[i].SetGameStat(effect.target, amount);
            }
        }

        return base.Activate(effect);
    }

    public override void OnExpire(AppliedEffect effect)
    {
        for (int i = 0; i < stats.Length; i++)
        {
            stats[i].SetGameStat(effect.target, -effect.cachedValues[i]);
        }

        base.OnExpire(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        string d = "";

        for (int i = 0; i < stats.Length; i++)
        {
            if (i > 0)
            {
                d += ", ";
            }

            if (effect.cachedValues[i] >= 0)
            {
                d += "increase ";
            }
            else
            {
                d += "decrease ";
            }

            d += stats[i].Name;

            d += " by ";

            switch (effect.values[0].deltaType)
            {
                case EffectProperties.EffectType.Percentage:
                    {
                        var abs = Mathf.Abs(effect.cachedValues[i]);
                        d += abs.FormatPercentage();
                    }
                    break;
                case EffectProperties.EffectType.Value:
                    var val = Mathf.FloorToInt(effect.cachedValues[i]);
                    d += Mathf.Abs(val);
                    break;
                case EffectProperties.EffectType.Decimal:
                    {
                        var abs = Mathf.Abs(effect.cachedValues[i]);
                        d += abs.FormatToDecimal();
                    }
                    break;
            }
        }

        d = d.Substring(0, 1).ToUpper() + d.Substring(1);

        return d;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string description = TargetModeDescriptor(targetMode);
    
        for (int i = 0; i < stats.Length; i++)
        {
            if (i > 0)
            {
                description += ", ";
            }
    
            description += stats[i].Name;
    
            if (props.effectValues[i].multiplier >= 0 && props.effectValues[i].flat >= 0)
            {
                description += " increased ";
            }
            else
            {
                description += " decreased ";
            }

            description += "by " + EffectValueDescriptor(props.effectValues[i]);
            
            description += DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
        }
    
        return description;
    }
}