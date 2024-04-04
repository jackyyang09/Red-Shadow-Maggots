﻿using System.Collections;
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

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        var s = base.GetSkillDescription(targetMode, props);

        return s + "every turn " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        var s = "Recovers " + Mathf.FloorToInt(effect.cachedValues[0]) + " " + RSMConstants.Keywords.Short.HEALTH + " ";

        return s + "every turn";
    }
}