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
        if (effect.valueCache.Count == 0)
        {
            var value = effect.valueGroup.GetValue(effect.targetProps);
            stat.SetGameStat(effect.Target, value);
            effect.valueCache.Add(new CachedValue { Value = value, Type = effect.valueGroup.ValueType });
        }
        else
        {
            var amount = effect.valueCache[0];
            stats[0].SetGameStat(effect.Target, amount.Value);
        }

        return base.Activate(effect);
    }

    public override void OnExpire(AppliedEffect effect)
    {
        stat.SetGameStat(effect.Target, -effect.valueCache[0].Value);

        base.OnExpire(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        if (!stat) return "No effect";

        string d = effectDescription;

        var key = "$STAT";

        var statName = stat ? stat.name : "NO STAT";
        d = d.Replace(key, statName);

        var value = effect.valueGroup.Values[0];
        d = value.ProcessSkillDescription(d, 0);

        return d;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        var d = base.GetSkillDescription(targetMode, props);

        var key = "$STAT";
        var statName = stat ? stat.name : "NO STAT";
        d = d.Replace(key, statName);

        var value = props.valueGroup.Values[0];
        d = value.ProcessSkillDescription(d, 0);

        return d + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);

        //string description = TargetModeDescriptor(targetMode);
        //
        //description += stat.Name;
        //
        //var value = props.effectValues[0];
        //
        //if (value.multiplier >= 0 && value.flat >= 0)
        //{
        //    description += " increased ";
        //}
        //else
        //{
        //    description += " decreased ";
        //}
        //
        //description += "by " + EffectValueDescriptor(value);
        //
        //description += DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
        //
        //return description;
    }
}