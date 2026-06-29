using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

[CreateAssetMenu(fileName = "Pisspenser Upgrade", menuName = "ScriptableObjects/Character-Specific/Piss/Pisspenser Upgrade", order = 1)]
public class PisspenserSpecial : BaseGameEffect, IStackableEffect
{
    public override string ExplainerName => "Upgrade";

    [SerializeReference, SubclassSelector] BaseGameStat[] stats;
    [SerializeField] float[] values;
    [SerializeReference, SubclassSelector] BaseEffectTarget targetMode;

    public override bool Activate(AppliedEffect effect)
    {
        effect.Caster.OnStartTurn += () => OnStartTurn(effect);
        return base.Activate(effect);
    }

    void OnStartTurn(AppliedEffect effect)
    {
        var targets = targetMode.GetTargets(effect.Caster, effect.Target);

        if (effect.cachedValues.Count > 0) // Remove old stat changes
        {
            stats[0].SetGameStat(effect.Caster, -effect.cachedValues[0].Value);
            stats[1].SetGameStat(effect.Caster, -effect.cachedValues[1].Value);
            effect.cachedValues.Clear();
        }

        for (int i = 0; i < stats.Length; i++)
        {
            var value = effect.Stacks * values[i] * stats[i].GetGameStat(effect.Caster);
            effect.cachedValues.Add(new CachedValue { Value = value, Type = ValueType.Percentage });
        }

        stats[0].SetGameStat(effect.Caster, effect.cachedValues[0].Value);
        stats[1].SetGameStat(effect.Caster, effect.cachedValues[1].Value);

        foreach (var t in targets)
        {
            t.Heal(effect.cachedValues[2].Value);
            t.GiveShield(effect.cachedValues[2].Value, effect);
        }
    }

    public void OnStacksChanged(AppliedEffect effect, int previous)
    {
        
    }
}