using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Multi Stat Stack Effect", menuName = "ScriptableObjects/Multi Stat Stack Effect", order = 1)]
public class MultiStatStackEffect : BaseGameEffect, IStackableEffect
{
    [SerializeReference, SubclassSelector] public BaseGameStat[] stats;
    [SerializeReference, SubclassSelector] protected BaseEffectValue[] values;

    public override bool Activate(AppliedEffect effect)
    {
        // Dummy
        // This class probably shouldn't inherit from StatChangeEffect
        return true;
    }

    /// <summary>
    /// This is a hack, this class should be re-implemented in a way that 
    /// doesn't include non-stack effects
    /// </summary>
    protected virtual void AdditionalEffects(AppliedEffect effect)
    {

    }

    protected void RemoveEffect(AppliedEffect effect)
    {
        for (int i = 0; i < effect.cachedValues.Count; i++)
        {
            stats[i].SetGameStat(effect.Target, -effect.cachedValues[i].Value);
        }

        effect.cachedValues.Clear();
    }

    public virtual void OnStacksChanged(AppliedEffect effect, int previous)
    {
        Debug.Log(effect.Stacks);
        if (previous > 0)
        {
            RemoveEffect(effect);
        }

        if (effect.Stacks == 0) return;

        if (effect.cachedValues.Count > 0)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i].SetGameStat(effect.Target, effect.cachedValues[i].Value);
            }
        }
        else
        {
            for (int i = 0; i < stats.Length; i++)
            {
                var amount = values[i].GetValue(effect.targetProps) * effect.Stacks;

                stats[i].SetGameStat(effect.Target, amount);

                effect.cachedValues.Add(new() { Value = amount, Type = ValueType.Value });
            }

            AdditionalEffects(effect);
        }

        GlobalEvents.OnGameEffectApplied?.Invoke(this);
    }
}