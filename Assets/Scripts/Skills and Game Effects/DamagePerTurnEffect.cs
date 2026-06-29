using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Per Turn Effect", menuName = "ScriptableObjects/Game Effects/Take Damage Per Turn", order = 1)]
public class DamagePerTurnEffect : BaseDamageEffect
{
    public override float TickAnimationTime => -1f;

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count == 0)
        {
            var value = effect.value.GetValue(effect.targetProps);
            effect.cachedValues.Add(new() { Value = value, Type = effect.value.ValueType});
        }

        return base.Activate(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        var d = base.GetEffectDescription(effect);
        d = d.Replace("$VALUE", effect.cachedValues[0].Value.FormatToDecimal());
        return d;
    }
}