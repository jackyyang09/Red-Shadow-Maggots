﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crit Chance Effect", menuName = "ScriptableObjects/Game Effects/Crit Chance", order = 1)]
public class BaseCritChanceBuff : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        EnemyCharacter enemy = target as EnemyCharacter;
        if (enemy)
        {
            enemy.DecreaseChargeLevel();
        }
        else
        {
            float percentageChange = (float)GetEffectStrength(strength, customValues);
            if (effectType == EffectType.Debuff) percentageChange *= -1;

            target.ApplyCritChanceModifier(percentageChange);
        }
    }

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);
        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.RemoveCritChanceModifier(percentageChange);
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        string s = TargetModeDescriptor(targetMode);

        s += "Crit Chance ";

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
                return customValues[0];
            case EffectStrength.Weak:
                return 0.1f;
            case EffectStrength.Small:
                return 0.2f;
            case EffectStrength.Medium:
                return 0.3f;
            case EffectStrength.Large:
                return 0.4f;
            case EffectStrength.EX:
                return 0.5f;
        }
        return 0;
    }
}