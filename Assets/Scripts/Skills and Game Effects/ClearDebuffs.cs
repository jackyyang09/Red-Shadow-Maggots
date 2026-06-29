using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Clear Debuffs", menuName = "ScriptableObjects/Game Effects/Clear Debuffs", order = 1)]
public class ClearDebuffs : BaseGameEffect
{
    public override bool Activate(AppliedEffect effect)
    {
        var effects = effect.Target.Effects.Where(e => e.referenceEffect.effectType == EffectType.Debuff).ToList();

        if (effects.Count == 0) return false;

        effects[0].Remove();

        return true;
    }
}
