using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiStatChangeEffect : BaseGameEffect
{
    [System.Serializable]
    public class StatChangePair
    {
        public BaseGameStat targetStat;
        public float value;
    }

    [SerializeField] protected List<StatChangePair> stats = new List<StatChangePair>();

    public override bool Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        foreach (var stat in stats)
        {
            stat.targetStat.SetGameStat(target, stat.value);
        }

        return base.Activate(user, target, strength, customValues);
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        foreach (var stat in stats)
        {
            stat.targetStat.SetGameStat(target, -stat.value);
        }

        base.OnExpire(user, target, strength, customValues);
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        string d = "";

        for (int i = 0; i < stats.Count; i++)
        {
            if (i > 0)
            {
                d += ", ";
            }

            if (stats[i].value >= 0)
            {
                d += "increase ";
            }
            else
            {
                d += "decrease ";
            }

            d += stats[i].targetStat.Name;

            d += " by " + stats[i].value.FormatPercentage() /* + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit)*/;
        }

        d = d.Substring(0, 1).ToUpper() + d.Substring(1);

        return d;
    }

    //public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    //{
    //    string description = TargetModeDescriptor(targetMode);
    //
    //    for (int i = 0; i < stats.Count; i++)
    //    {
    //        if (i > 0)
    //        {
    //            description += ", ";
    //        }
    //
    //        description += stats[i].targetStat.Name;
    //
    //        if (stats[i].value.Table[0] >= 0)
    //        {
    //            description += " increased ";
    //        }
    //        else
    //        {
    //            description += " decreased ";
    //        }
    //
    //        description += "by " + stats[i].value.GetDescription(props.strength) + " " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    //    }
    //
    //    return description;
    //}
}
