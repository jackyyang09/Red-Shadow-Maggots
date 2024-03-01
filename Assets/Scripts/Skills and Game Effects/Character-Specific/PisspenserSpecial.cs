using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

[CreateAssetMenu(fileName = "Pisspenser Upgrade", menuName = "ScriptableObjects/Character-Specific/Piss/Pisspenser Upgrade", order = 1)]
public class PisspenserSpecial : MultiStatStackEffect
{
    public override string ExplainerName => "Upgrade";

    [SerializeField] EffectProperties healEffect;
    [SerializeField] EffectProperties shieldEffect;

    public override bool Activate(AppliedEffect effect)
    {
        effect.customCallbacks = new System.Action[1];
        effect.customCallbacks[0] = () => OnSpecialCallback(effect);
        effect.target.OnStartTurnLate += effect.customCallbacks[0];

        if (effect.target.IsPlayer())
        {
            effect.extraTargets = battleSystem.PlayerList.ConvertAll(e => (BaseCharacter)e);
        }
        else
        {
            effect.extraTargets = enemyController.EnemyList.ConvertAll(e => (BaseCharacter)e);
        }
        effect.extraTargets.Remove(effect.target);

        return true;
    }

    public override void OnExpire(AppliedEffect effect)
    {
        effect.target.OnStartTurnLate -= effect.customCallbacks[0];

        base.OnExpire(effect);
    }

    public override void OnSpecialCallback(AppliedEffect effect)
    {
        base.OnSpecialCallback(effect);

        var targets = new List<BaseCharacter>{ effect.target };
        targets.AddRange(effect.extraTargets);

        foreach (var ally in targets)
        {
            var props = healEffect.Copy();
            props.effectValues = new[] { effect.values[2] * effect.Stacks };
            BaseCharacter.ApplyEffectToCharacter(props, effect.caster, ally);

            props = shieldEffect.Copy();
            props.effectValues = new[] { effect.values[3] * effect.Stacks };
            BaseCharacter.ApplyEffectToCharacter(props, effect.caster, ally);
        }
    }

    public new void OnStacksChanged(AppliedEffect effect)
    {
        var targets = new List<BaseCharacter> { effect.target };
        targets.AddRange(effect.extraTargets);

        foreach (var ally in targets)
        {
            for (int i = 2; i < effect.cachedValues.Count; i++)
            {
                stats[i].SetGameStat(ally, -effect.cachedValues[i]);
            }
        }

        effect.cachedValues.Clear();

        foreach (var ally in targets)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                var amount = GetValue(stats[i], effect.values[i + 2], ally) * effect.Stacks;

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