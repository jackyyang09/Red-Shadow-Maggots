using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Defense Effect", menuName = "ScriptableObjects/Game Effects/Defense", order = 1)]
public class BaseDefenseEffect : BaseGameEffect
{
    public override bool Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyDefenseModifier(percentageChange);

        return true;
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyDefenseModifier(-percentageChange);
    }   

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        float percentageChange = (float)GetEffectStrength(props.strength, props.customValues);

        string s = "";

        if (effectType == EffectType.Debuff)
            s += "Decrease ";
        else
            s += "Increase ";

        s += TargetModeDescriptor(targetMode);
        s += RSMConstants.Keywords.Short.DEFENSE + " by " + percentageChange * 100 + "% ";

        return s + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
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