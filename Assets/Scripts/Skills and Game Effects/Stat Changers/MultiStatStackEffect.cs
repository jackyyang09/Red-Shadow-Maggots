using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Multi Stat Stack Effect", menuName = "ScriptableObjects/Multi Stat Stack Effect", order = 1)]
public class MultiStatStackEffect : MultiStatChangeEffect, IStackableEffect
{
    public void OnStacksChanged(AppliedEffect effect)
    {
        for (int i = 0; i < effect.cachedValues.Count; i++)
        {
            stats[i].SetGameStat(effect.target, -effect.cachedValues[i]);
        }

        effect.cachedValues.Clear();

        for (int i = 0; i < stats.Length; i++)
        {
            var amount = GetValue(stats[i], effect.values[i], effect.target) * effect.Stacks;

            stats[i].SetGameStat(effect.target, amount);

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
                d += "receive ";
                break;
        }

        d += props.stacks + " stacks of " + props.effect.effectName;

        return d + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }
}