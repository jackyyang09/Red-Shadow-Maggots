using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This code is terrible
/// Effects are usually applied through AppliedEffect, but each AppliedEffect created is treated as a different entity
/// Having one "effect" control multiple "effects" creates a mess that I didn't account for
/// </summary>
[CreateAssetMenu(fileName = "New Stack Sequence", menuName = "ScriptableObjects/Stack Sequence Effect", order = 1)]
public class StackSequenceEffect : CompoundEffect, IStackableEffect
{
    [SerializeReference, SubclassSelector] public BaseGameStat[] stats;

    [SerializeReference, SubclassSelector] public BaseEffectValue[] values;

    public int[] stackRequirements;

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count > 0)
        {
            effect.ResetDuration();
        
            for (int i = 0; i < effect.cachedValues.Count; i++)
            {
                stats[i].SetGameStat(effect.Target, effect.cachedValues[i].Value);
            }
        }
        else
        {
            for (int i = 0; i < stackRequirements.Length; i++)
            {
                if (effect.Stacks >= stackRequirements[i])
                {
                    var amount = values[i].GetValue(effect.targetProps);

                    stats[i].SetGameStat(effect.Target, amount);

                    effect.cachedValues.Add(new() { Value = amount, Type = values[i].ValueType });
                }
            }
        }

        return true;
    }

    //public override void Tick(AppliedEffect effect)
    //{
    //    effect.Stacks--;
    //    if (effect.Stacks == 0)
    //    {
    //        effect.Remove();
    //    }
    //    base.Tick(effect);
    //}

    public override void OnExpire(AppliedEffect effect)
    {
        for (int i = 0; i < effect.cachedValues.Count; i++)
        {
            stats[i].SetGameStat(effect.Target, -effect.cachedValues[i].Value);
        }
    }

    public void OnStacksChanged(AppliedEffect effect, int previous)
    {
        var targets = new List<BaseCharacter> { effect.Target };

        if (effect.cachedValues.Count > 0)
        {
            OnExpire(effect);
        }

        effect.cachedValues.Clear();

        Activate(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        string d = effectDescription + "\n";
        for (int i = 0; i < stats.Length; i++)
        {
            //effect.valueGroup = props.value.ShallowCopy();

            var v = values[i].GetValue(effect.targetProps);
            bool positive = v > 0;
            var skillD = positive ? "Increase " : "Decrease ";
            skillD += stats[i].Name + " ";

            string value = "(";
            value += positive ? "+" : "\u2011"; // Non-breaking hyphen
            value += values[i].Descriptor + ")";

            if (i < effect.Stacks)
            {
                d += stackRequirements[i] + " - " + skillD + value;
            }
            else
            {
                d += "<color=grey>" + stackRequirements[i] + " - " + skillD + value + "</color>";
            }

            d += "\n";

            //Queue<float> cache = new Queue<float>(effect.cachedValues);
            //for (int j = 0; j < props.effect.ValueCount; j++)
            //{
            //    cache.Enqueue(cache.Dequeue());
            //}
            //effect.cachedValues = new List<float>(cache);
        }

        return d;
    }
}