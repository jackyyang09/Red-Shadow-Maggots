using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Compound Effect", menuName = "ScriptableObjects/Game Effects/Compound Effect", order = 1)]
public class CompoundEffect : BaseGameEffect
{
    [SerializeReference] public EffectGroup[] effectGroups;

    public override int ValueCount
    {
        get
        {
            int count = 0;
            foreach (EffectGroup p in effectGroups)
            {
                count += p.effectProps.effect.ValueCount;
            }
            return count;
        }
    }

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count == 0)
        {
            for (int i = 0; i < effectGroups.Length; i++)
            {
                var cached = new List<float>(effect.cachedValues);
                effect.cachedValues.Clear();
                effectGroups[i].effectProps.effect.Activate(effect);
                cached.AddRange(effect.cachedValues);
                effect.cachedValues = cached;
            }
        }
        else
        {
            for (int i = 0; i < effectGroups.Length; i++)
            {
                effectGroups[i].effectProps.effect.Activate(effect);
                Queue<float> cache = new Queue<float>(effect.cachedValues);
                for (int j = 0; j < effectGroups[i].effectProps.effect.ValueCount; j++)
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
        for (int i = 0; i < effectGroups.Length; i++)
        {
            effectGroups[i].effectProps.effect.Tick(effect);
        }

        base.Tick(effect);
    }

    public override void OnExpire(AppliedEffect effect)
    {
        for (int i = 0; i < effectGroups.Length; i++)
        {
            effectGroups[i].effectProps.effect.OnExpire(effect);
        }

        base.OnExpire(effect);
    }

    //public override string GetEffectDescription(AppliedEffect effect)
    //{
    //    if (effects.Length == 0) return "No effect";
    //    
    //    string d = "";
    //    
    //    for (int i = 0; i < effects.Length; i++)
    //    {
    //        d += effects[i].GetEffectDescription(effect);
    //    }
    //    
    //    d = d.Substring(0, 1).ToUpper() + d.Substring(1);
    //
    //    return d;
    //}

    //public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    //{
    //    string d = TargetModeDescriptor(targetMode);
    //
    //    for (int i = 0; i < effects.Length; i++)
    //    {
    //        d += effects[i].GetSkillDescription(targetMode, props);
    //    }
    //
    //    return d;
    //}
}