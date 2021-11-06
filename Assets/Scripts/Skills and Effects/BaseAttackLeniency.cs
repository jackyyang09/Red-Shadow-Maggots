using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Leniency Buff", menuName = "ScriptableObjects/Game Effects/Attack Leniency", order = 1)]
public class BaseAttackLeniency : BaseGameEffect
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

        ((PlayerCharacter)target).attackLeniencyModifier += percentageChange;
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

        ((PlayerCharacter)target).attackLeniencyModifier -= percentageChange;
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
            return "Attack Window Reduced by " + percentageChange * 100 + "%";
        else
            return "Attack Window Increased by " + percentageChange * 100 + "%";
    }
}
