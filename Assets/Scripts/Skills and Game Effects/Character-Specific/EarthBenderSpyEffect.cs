using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Earth Bender Special", menuName = "ScriptableObjects/Character-Specific/Earth Bender Special", order = 1)]
public class EarthBenderSpyEffect : MultiStatChangeEffect, IStackableEffect
{
    public void OnStacksChanged(AppliedEffect effect)
    {
        if (effect.cachedValue.Count > 0)
        {
            for (int i = 0; i < effect.cachedValue.Count; i++)
            {
                stats[i].targetStat.SetGameStat(effect.target, -effect.cachedValue[i]);
            }
        }

        foreach (var stat in stats)
        {
            var amount = stat.value * effect.Stacks;

            stat.targetStat.SetGameStat(effect.target, amount);

            effect.cachedValue.Add(amount);
        }
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        string d = "Each stack ";

        for (int i = 0; i < stats.Count; i++)
        {
            if (i > 0)
            {
                d += ", ";
            }

            if (stats[i].value >= 0)
            {
                d += "increases ";
            }
            else
            {
                d += "decreases ";
            }

            d += stats[i].targetStat.Name;
            
            d += " by " + Mathf.Abs(stats[i].value).FormatPercentage() /*  + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit)*/;
        }

        return d;
    }
}