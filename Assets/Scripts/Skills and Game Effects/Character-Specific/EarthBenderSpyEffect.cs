using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Earth Bender Special", menuName = "ScriptableObjects/Character-Specific/Earth Bender Special", order = 1)]
public class EarthBenderSpyEffect : MultiStatChangeEffect, IStackableEffect
{
    public void OnStacksChanged(AppliedEffect effect)
    {
        for (int i = 0; i < effect.cachedValue.Count; i++)
        {
            stats[i].targetStat.SetGameStat(effect.target, -effect.cachedValue[i]);
        }

        effect.cachedValue.Clear();

        for (int i = 0; i < stats.Count; i++)
        {
            var amount = stats[i].value * effect.Stacks;

            stats[i].targetStat.SetGameStat(effect.target, amount);

            effect.cachedValue.Add(amount);
        }
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        base.OnExpire(user, target, strength, customValues);
        Debug.Log("WTF???");
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