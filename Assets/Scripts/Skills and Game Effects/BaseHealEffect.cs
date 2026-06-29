using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal", order = 1)]
public class BaseHealEffect : BaseGameEffect
{
    protected void ApplyHeal(AppliedEffect effect)
    {
        effect.Target.Heal(effect.cachedValues[0].Value);
    }

    protected void CacheValue(AppliedEffect effect)
    {
        if (effect.cachedValues.Count == 0)
        {
            //effect.cachedValues.Add(GetValue(stat, effect.values[0], effect.caster));
            effect.cachedValues.Add(new() { Value = effect.value.GetValue(effect.targetProps), Type = ValueType.Value });
        }
    }

    public override bool Activate(AppliedEffect effect)
    {
        CacheValue(effect);

        ApplyHeal(effect);

        return true;
    }
}