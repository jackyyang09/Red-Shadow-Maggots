using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Buff", menuName = "ScriptableObjects/Game Effects/Attack Damage", order = 1)]
public class BaseAttackBuff : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyAttackModifier(percentageChange);
    }


    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);
        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyAttackModifier(-percentageChange);
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        string s = TargetModeDescriptor(targetMode);

        s += "Attack Damage ";

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
                if (customValues.Length > 0) return customValues[0];
                else return 0f;
            case EffectStrength.Weak:
                return 0.05f;
            case EffectStrength.Small:
                return 0.1f;
            case EffectStrength.Medium:
                return 0.2f;
            case EffectStrength.Large:
                return 0.3f;
            case EffectStrength.EX:
                return 0.5f;
        }
        return 0;
    }
}