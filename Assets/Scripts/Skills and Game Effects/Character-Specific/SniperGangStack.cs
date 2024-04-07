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

    public override void OnStacksChanged(AppliedEffect effect)
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
        string d = "Apply the following effects based on the number of stacks:\n";

        d += "1 - Reduce " + RSMConstants.Keywords.Short.ATTACK + " by " +
            effect.values[0].multiplier.FormatPercentage() + "\n";

        d += "2 - Reduce " + RSMConstants.Keywords.Short.DEFENSE + " by " +
            effect.values[1].multiplier.FormatPercentage() + "\n";

        d += "3 - Reduce " + RSMConstants.Keywords.Short.WAIT_LIMIT + " by " +
            effect.values[2].flat.FormatToDecimal() + "\n";

        d += "4 - Increase " + RSMConstants.Keywords.Short.WAIT + " by " +
            effect.values[3].flat.FormatToDecimal() + "\n";

        return d;
    }
}