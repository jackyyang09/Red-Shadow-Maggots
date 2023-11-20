using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Effect", menuName = "ScriptableObjects/Game Effects/Attack Effect", order = 1)]
public class BaseAttackEffect : BaseGameEffect
{
    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);
        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyAttackModifier(-percentageChange);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        string s = TargetModeDescriptor(targetMode);

        return "Deal damage equal to " + percentageChange * 100 + "% of Attack";
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                if (customValues.Length > 0) return customValues[0];
                else return 0f;
            case EffectStrength.Weak:
                return 2f;
            case EffectStrength.Small:
                return 3f;
            case EffectStrength.Medium:
                return 4f;
            case EffectStrength.Large:
                return 5f;
            case EffectStrength.EX:
                return 6f;
        }
        return 0;
    }
}