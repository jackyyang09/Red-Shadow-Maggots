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
            //var value = GetValue(stat, effect.values[0], effect.Caster);
            var value = effect.valueGroup.GetValue(effect.targetProps);
            effect.cachedValues.Add(value);
        }

        return base.Activate(effect);
    }

    //public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    //{
    //    var s = base.GetSkillDescription(targetMode, props);
    //
    //    return s + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    //}
}