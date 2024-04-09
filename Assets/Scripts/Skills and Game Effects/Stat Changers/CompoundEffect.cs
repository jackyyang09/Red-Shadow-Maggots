using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Compound Effect", menuName = "ScriptableObjects/Game Effects/Compound Effect", order = 1)]
public class CompoundEffect : BaseGameEffect
{
    public BaseGameEffect[] effects;
    public override int ValueCount => effects.Length;

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count == 0)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                var cached = new List<float>(effect.cachedValues);
                effect.cachedValues.Clear();
                effects[i].Activate(effect);
                cached.AddRange(effect.cachedValues);
                effect.cachedValues = cached;
            }
        }
        else
        {
            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].Activate(effect);
                Queue<float> cache = new Queue<float>(effect.cachedValues);
                for (int j = 0; j < effects[i].ValueCount; j++)
                {
                    cache.Enqueue(cache.Dequeue());
                }
                effect.cachedValues = new List<float>(cache.ToArray());
            }
        }

        return base.Activate(effect);
    }

    public override void Tick(AppliedEffect effect)
    {
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].Tick(effect);
        }

        base.Tick(effect);
    }

    public override void OnExpire(AppliedEffect effect)
    {
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].OnExpire(effect);
        }

        base.OnExpire(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        if (effects.Length == 0) return "No effect";
        
        string d = "";
        
        for (int i = 0; i < effects.Length; i++)
        {
            d += effects[i].GetEffectDescription(effect);
        }
        
        d = d.Substring(0, 1).ToUpper() + d.Substring(1);

        return d;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string d = TargetModeDescriptor(targetMode);

        for (int i = 0; i < effects.Length; i++)
        {
            d += effects[i].GetSkillDescription(targetMode, props);
        }

        return d;
    }
}