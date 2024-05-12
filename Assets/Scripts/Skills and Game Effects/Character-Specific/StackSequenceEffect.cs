using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stack Sequence", menuName = "ScriptableObjects/Stack Sequence Effect", order = 1)]
public class StackSequenceEffect : CompoundEffect, IStackableEffect
{
    public int[] stackRequirements;

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.valueCache.Count > 0)
        {
            effect.ResetDuration();

            for (int i = 0; i < effect.Stacks; i++)
            {
                effectGroups[i].effectProps.effect.Activate(effect);
                Queue<CachedValue> cache = new Queue<CachedValue>(effect.valueCache);
                for (int j = 0; j < effectGroups[i].effectProps.effect.ValueCount; j++)
                {
                    cache.Enqueue(cache.Dequeue());
                }
                effect.valueCache = new List<CachedValue>(cache.ToArray());
            }
        }
        else
        {
            effect.customCallbacks = new System.Action[1];
            effect.customCallbacks[0] = () => OnSpecialCallback(effect);
            effect.Target.OnStartTurnLate += effect.customCallbacks[0];

            Queue<BaseEffectValue> backup = new(effect.valueGroup.Values);
            for (int i = 0; i < effect.Stacks; i++)
            {
                effect.valueGroup = effectGroups[i].effectProps.valueGroup.ShallowCopy();

                var cached = new List<CachedValue>(effect.valueCache);
                effect.valueCache.Clear();
                effectGroups[i].effectProps.effect.Activate(effect);
                cached.AddRange(effect.valueCache);
                effect.valueCache = cached;
            }
            effect.valueGroup.Values = backup.ToArray();
        }

        return true;
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
        if (effect.valueCache.Count > 0)
        {
            effect.Target.OnStartTurnLate -= effect.customCallbacks[0];

            var count = effect.valueCache.Count;
            for (int i = 0; i < count; i++)
            {
                effectGroups[i].effectProps.effect.OnExpire(effect);

                effect.valueCache.RemoveAt(0);
            }
        }
    }

    public void OnStacksChanged(AppliedEffect effect)
    {
        var targets = new List<BaseCharacter> { effect.Target };
        targets.AddRange(effect.extraTargets);

        if (effect.valueCache.Count > 0)
        {
            OnExpire(effect);
        }

        effect.valueCache.Clear();

        Activate(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        string d = effectDescription + "\n";
    
        for (int i = 0; i < effectGroups.Length; i++)
        {
            var props = effectGroups[i].effectProps;
            effect.valueGroup = props.valueGroup.ShallowCopy();

            var skillD = props.effect.GetSkillDescription(TargetMode.None, props);

            if (i < effect.Stacks)
            {
                d += stackRequirements[i] + " - " + skillD;
            }
            else
            {
                d += "<color=grey>" + stackRequirements[i] + " - " + skillD + "</color>";
            }

            if (i < effect.valueCache.Count)
            {
                var value = effect.valueCache[i];

                d += " (";
                d += value.Value >= 0 ? "+" : "\u2011"; // Non-breaking hyphen
                d += value.Value.FormatTo(value.Type) + ")";
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

    //public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    //{
    //    string d = "Apply " + props.stacks + " Stack of " + props.effect.effectName + " to "
    //        + TargetModeDescriptor(targetMode);
    //
    //    return d;
    //}
}