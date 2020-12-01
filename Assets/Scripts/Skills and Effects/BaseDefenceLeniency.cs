using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Defence Leniency Buff", menuName = "ScriptableObjects/Skills/Defence Leniency", order = 1)]
public class BaseDefenceLeniency : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = 0;
        switch (strength)
        {
            case EffectStrength.Custom:
                percentageChange = customValues[0];
                break;
            case EffectStrength.Weak:
                percentageChange = 0.05f;
                break;
            case EffectStrength.Small:
                percentageChange = 0.15f;
                break;
            case EffectStrength.Medium:
                percentageChange = 0.3f;
                break;
            case EffectStrength.Large:
                percentageChange = 0.45f;
                break;
            case EffectStrength.EX:
                percentageChange = 1f;
                break;
        }
        if (effectType == EffectType.Debuff) percentageChange *= -1;

        ((PlayerCharacter)target).defenceLeniencyModifier += percentageChange;
    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = 0;
        switch (strength)
        {
            case EffectStrength.Custom:
                percentageChange = customValues[0];
                break;
            case EffectStrength.Weak:
                percentageChange = 0.05f;
                break;
            case EffectStrength.Small:
                percentageChange = 0.15f;
                break;
            case EffectStrength.Medium:
                percentageChange = 0.3f;
                break;
            case EffectStrength.Large:
                percentageChange = 0.45f;
                break;
            case EffectStrength.EX:
                percentageChange = 1f;
                break;
        }
        if (effectType == EffectType.Debuff) percentageChange *= -1;

        ((PlayerCharacter)target).defenceLeniencyModifier -= percentageChange;
    }

    public override void Tick()
    {

    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        float percentageChange = 0;
        switch (strength)
        {
            case EffectStrength.Custom:
                percentageChange = customValues[0];
                break;
            case EffectStrength.Weak:
                percentageChange = 0.05f;
                break;
            case EffectStrength.Small:
                percentageChange = 0.15f;
                break;
            case EffectStrength.Medium:
                percentageChange = 0.3f;
                break;
            case EffectStrength.Large:
                percentageChange = 0.45f;
                break;
            case EffectStrength.EX:
                percentageChange = 1f;
                break;
        }

        if (effectType == EffectType.Debuff)
            return "Defence Window Reduced by " + percentageChange * 100 + "%";
        else
            return "Defence Window Increased by " + percentageChange * 100 + "%";
    }
}
