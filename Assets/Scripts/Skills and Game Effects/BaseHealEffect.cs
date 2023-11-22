﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal", order = 1)]
public class BaseHealEffect : BaseGameEffect
{
    public GameStatValue value;

    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        var amount = value.SolveValue(strength, user, target);

        //var amount = (float)GetEffectStrength(strength, customValues) * user.MaxHealth;

        target.Heal(amount);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        var change = (int)((float)GetEffectStrength(props.strength, props.customValues) * 100);

        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += "recovers ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "recover ";
                break;
        }

        s += Keywords.Short.HEALTH + " equal to " +
            change + "% of your " + Keywords.Short.MAX_HEALTH;

        return s;
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                if (customValues.Length == 0)
                {
                    return 0f;
                }
                else return (float)customValues[0];
            case EffectStrength.Weak:
                return 0.05f;
            case EffectStrength.Small:
                return 0.15f;
            case EffectStrength.Medium:
                return 0.3f;
            case EffectStrength.Large:
                return 0.6f;
            case EffectStrength.EX:
                return 1f;
        }
        return 0;
    }
}
