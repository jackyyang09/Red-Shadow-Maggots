﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Skills/Heal", order = 1)]
public class BaseHealEffect : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float healAmount = 0;
        switch (strength)
        {
            case EffectStrength.Custom:
                healAmount = customValues[0];
                break;
            case EffectStrength.Weak:
                healAmount = 800f;
                break;
            case EffectStrength.Small:
                healAmount = 1500f;
                break;
            case EffectStrength.Medium:
                healAmount = 2000;
                break;
            case EffectStrength.Large:
                healAmount = 3000;
                break;
            case EffectStrength.EX:
                healAmount = 5000;
                break;
        }

        target.Heal(healAmount);
    }

    public override void Tick()
    {

    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {

    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        float healAmount = 0;
        switch (strength)
        {
            case EffectStrength.Custom:
                healAmount = customValues[0];
                break;
            case EffectStrength.Weak:
                healAmount = 800f;
                break;
            case EffectStrength.Small:
                healAmount = 1500f;
                break;
            case EffectStrength.Medium:
                healAmount = 2000;
                break;
            case EffectStrength.Large:
                healAmount = 3000;
                break;
            case EffectStrength.EX:
                healAmount = 5000;
                break;
        }
        return "Heal " + healAmount + " per turn";
    }
}
