using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sniper Gang Stack", menuName = "ScriptableObjects/Character-Specific/Sniper Gang Stack", order = 1)]
public class SniperGangStack : MultiStatStackEffect
{
    public override string ExplainerName => "Cripple";

    public override bool Activate(AppliedEffect effect)
    {
        effect.ResetDuration();
        return base.Activate(effect);
    }

    public override void Tick(AppliedEffect effect)
    {
        effect.Stacks--;
        if (effect.Stacks == 0)
        {
            effect.Remove();
        }
        base.Tick(effect);
    }

    public override void OnExpire(AppliedEffect effect)
    {
        effect.target.OnStartTurnLate -= effect.customCallbacks[0];

        base.OnExpire(effect);
    }

    public new void OnStacksChanged(AppliedEffect effect)
    {
        var targets = new List<BaseCharacter> { effect.target };
        targets.AddRange(effect.extraTargets);

        foreach (var ally in targets)
        {
            for (int i = 0; i < effect.cachedValues.Count; i++)
            {
                stats[i].SetGameStat(ally, -effect.cachedValues[i]);
            }
        }

        effect.cachedValues.Clear();

        foreach (var ally in targets)
        {
            for (int i = 0; i < effect.Stacks; i++)
            {
                var amount = GetValue(stats[i], effect.values[i], ally);

                stats[i].SetGameStat(ally, amount);

                effect.cachedValues.Add(amount);
            }
        }
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        string d = "For each stack: Increase " +
            RSMConstants.Keywords.Short.ATTACK + " by " +
            effect.values[0].multiplier.FormatPercentage() +
            " and " + RSMConstants.Keywords.Short.DEFENSE + " by " +
            effect.values[1].multiplier.FormatPercentage() +
            " for " + TargetModeDescriptor(TargetMode.AllAllies).TrimEnd() + ". " +
            "At the start of your turn, " + TargetModeDescriptor(TargetMode.AllAllies) + "recover " +
            RSMConstants.Keywords.Short.HEALTH + " equal to " +
            effect.values[2].multiplier.FormatPercentage() + " of your " +
            RSMConstants.Keywords.Short.MAX_HEALTH +
            " and receive a Shield with a strength of " +
            effect.values[3].multiplier.FormatPercentage() + " of your " +
            RSMConstants.Keywords.Short.MAX_HEALTH + ".";

        return d;
    }
}