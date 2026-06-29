using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal Per Turn", order = 1)]
public class HealPerTurnEffect : BaseHealEffect
{
    public override bool Activate(AppliedEffect effect)
    {
        CacheValue(effect);
        return true;
    }

    public override void Tick(AppliedEffect effect)
    {
        base.Tick(effect);
        ApplyHeal(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        var d = base.GetEffectDescription(effect);
        d = d.Replace("$REGEN", effect.cachedValues[0].String());

        return d;
    }
}