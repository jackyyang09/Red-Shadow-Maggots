using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Multi Stat Stack Effect", menuName = "ScriptableObjects/Multi Stat Stack Effect", order = 1)]
public class MultiStatStackEffect : StatChangeEffect, IStackableEffect
{
    public virtual void OnStacksChanged(AppliedEffect effect)
    {
        for (int i = 0; i < effect.cachedValues.Count; i++)
        {
            stats[i].SetGameStat(effect.Target, -effect.cachedValues[i]);
        }

        effect.cachedValues.Clear();

        for (int i = 0; i < stats.Length; i++)
        {
            var amount = GetValue(stats[i], effect.values[i], effect.Target) * effect.Stacks;

            stats[i].SetGameStat(effect.Target, amount);

            effect.cachedValues.Add(amount);
        }
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        var d = base.GetEffectDescription(effect);

        return "For each stack, " + d.Substring(0, 1).ToLower() + d.Substring(1);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        var d = TargetModeDescriptor(targetMode);

        if (props.stacks > 0)
        {
            switch (targetMode)
            {
                case TargetMode.None:
                case TargetMode.Self:
                    d += "Receive ";
                    break;
                case TargetMode.OneAlly:
                case TargetMode.OneEnemy:
                case TargetMode.AllAllies:
                case TargetMode.AllEnemies:
                    d += "receives ";
                    break;
            }
        }
        else
        {
            switch (targetMode)
            {
                case TargetMode.None:
                case TargetMode.Self:
                    d += "Lose ";
                    break;
                case TargetMode.OneAlly:
                case TargetMode.OneEnemy:
                case TargetMode.AllAllies:
                case TargetMode.AllEnemies:
                    d += "loses ";
                    break;
            }
        }

        var count = Mathf.Abs(props.stacks);
        d += count + " stack";
        if (count > 1) d += "s";
        d += " of " + props.effect.effectName + " ";
        if (props.maxStacks > 0) d += "(Max " + props.maxStacks + ") ";

        return d + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }
}