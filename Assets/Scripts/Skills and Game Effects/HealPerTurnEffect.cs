﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal Per Turn", order = 1)]
public class HealPerTurnEffect : BaseHealEffect
{
    public override void Tick(AppliedEffect effect)
    {
        base.Tick(effect);
        base.Activate(effect);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        var s = base.GetSkillDescription(targetMode, props);

        return s + " every turn " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        var s = "Recovers " + RSMConstants.Keywords.Short.HEALTH + 
            " equal to " + value.GetDescription(strength, value.Stat);

        return s;
    }
}