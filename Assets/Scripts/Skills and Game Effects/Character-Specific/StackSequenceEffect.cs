using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stack Sequence", menuName = "ScriptableObjects/Stack Sequence Effect", order = 1)]
public class StackSequenceEffect : CompoundEffect, IStackableEffect
{
    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count > 0)
        {
            effect.ResetDuration();

            for (int i = 0; i < effect.Stacks; i++)
            {
                effects[i].Activate(effect);
                Queue<float> cache = new Queue<float>(effect.cachedValues);
                for (int j = 0; j < effects[i].ValueCount; j++)
                {
                    cache.Enqueue(cache.Dequeue());
                }
                effect.cachedValues = new List<float>(cache.ToArray());

                string d = "";
                foreach (var item in effect.cachedValues)
                {
                    d += item + ", ";
                }
            }
        }
        else
        {
            effect.customCallbacks = new System.Action[1];
            effect.customCallbacks[0] = () => OnSpecialCallback(effect);
            effect.target.OnStartTurnLate += effect.customCallbacks[0];

            Queue<EffectProperties.EffectValue> backup = new(effect.values);
            for (int i = 0; i < effect.Stacks; i++)
            {
                var cached = new List<float>(effect.cachedValues);
                effect.cachedValues.Clear();
                effects[i].Activate(effect);
                cached.AddRange(effect.cachedValues);
                effect.cachedValues = cached;

                Queue<EffectProperties.EffectValue> cache = new(effect.values);
                cache.Enqueue(cache.Dequeue());
                effect.values = cache.ToArray();
            }
            effect.values = backup.ToArray();
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
        if (effect.cachedValues.Count > 0)
        {
            effect.target.OnStartTurnLate -= effect.customCallbacks[0];

            var count = effect.cachedValues.Count;
            for (int i = 0; i < count; i++)
            {
                effects[i].OnExpire(effect);

                effect.cachedValues.RemoveAt(0);
            }
        }
    }

    public void OnStacksChanged(AppliedEffect effect)
    {
        var targets = new List<BaseCharacter> { effect.target };
        targets.AddRange(effect.extraTargets);

        if (effect.cachedValues.Count > 0)
        {
            OnExpire(effect);
        }

        effect.cachedValues.Clear();

        Activate(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        string d = "Apply the following effects based on the number of stacks:\n";

        var props = effect.Properties;
        for (int i = 0; i < effects.Length; i++)
        {
            d += (i + 1) + " - " + effects[i].GetSkillDescription(effect.targetMode, props) + "\n";

            Queue<EffectProperties.EffectValue> cache = new(props.effectValues);
            for (int j = 0; j < effects[i].ValueCount; j++)
            {
                cache.Enqueue(cache.Dequeue());
            }
            props.effectValues = cache.ToArray();
        }

        return d;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string d = "Apply " + props.stacks + " Stack of " + props.effect.effectName + " to "
            + TargetModeDescriptor(targetMode);

        return d;
    }
}