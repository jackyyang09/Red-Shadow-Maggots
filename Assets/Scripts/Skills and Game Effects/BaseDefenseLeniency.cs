﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Defense Leniency Buff", menuName = "ScriptableObjects/Game Effects/Defense Leniency", order = 1)]
public class BaseDefenseLeniency : BaseGameEffect
{
    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        PlayerCharacter t = target as PlayerCharacter;
        if (t) // This is a player
        {
            t.ApplyDefenseLeniencyModifier(percentageChange);
        }
        else // Enemies don't have Defense Leniency
        {
            Debug.LogError("TODO: Enemies don't have Defense Leniency!");
        }
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        PlayerCharacter t = target as PlayerCharacter;
        if (t) // This is a player
        {
            t.ApplyDefenseLeniencyModifier(-percentageChange);
        }
        else // Enemies don't have Defense Leniency
        {
            Debug.LogError("TODO: Enemies don't have Defense Leniency!");
        }
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        string s = TargetModeDescriptor(targetMode);

        s += "Defense Window ";

        if (effectType == EffectType.Debuff)
            s += "reduced";
        else
            s += "increased";

        return s + " by " + percentageChange * 100 + "% " + DurationDescriptor(duration);
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return customValues.Length > 0 ? customValues[0] : 0;
            case EffectStrength.Weak:
                return 0.05f;
            case EffectStrength.Small:
                return 0.15f;
            case EffectStrength.Medium:
                return 0.3f;
            case EffectStrength.Large:
                return 0.45f;
            case EffectStrength.EX:
                return 1f;
        }
        return 0;
    }
}
