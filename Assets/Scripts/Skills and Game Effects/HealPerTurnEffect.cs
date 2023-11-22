using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal Per Turn", order = 1)]
public class HealPerTurnEffect : BaseHealEffect
{
    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void Tick(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        base.Tick(user, target, strength, customValues);
        base.Activate(user, target, strength, customValues);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        var s = base.GetSkillDescription(targetMode, props);

        return s + " every turn " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }
}