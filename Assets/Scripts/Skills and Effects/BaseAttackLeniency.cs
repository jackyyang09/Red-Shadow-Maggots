using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Leniency Buff", menuName = "ScriptableObjects/Game Effects/Attack Leniency", order = 1)]
public class BaseAttackLeniency : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);
        if (effectType == EffectType.Debuff) percentageChange *= -1;

        ((PlayerCharacter)target).attackLeniencyModifier += percentageChange;
    }

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);
        if (effectType == EffectType.Debuff) percentageChange *= -1;

        ((PlayerCharacter)target).attackLeniencyModifier -= percentageChange;
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff)
            return "Attack Window Reduced by " + percentageChange * 100 + "%";
        else
            return "Attack Window Increased by " + percentageChange * 100 + "%";
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return customValues[0];
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
