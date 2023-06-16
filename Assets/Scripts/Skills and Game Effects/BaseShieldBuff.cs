using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseShieldBuff : BaseGameEffect
{
    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyDefenseModifier(percentageChange);
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyDefenseModifier(-percentageChange);
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.None:
            case TargetMode.Self:
                s = "Receive ";
                break;
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += "receives ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "receive ";
                break;
        }

        s += percentageChange * 100;

        if (effectType == EffectType.Debuff)
            s += "% more ";
        else
            s += "% less ";


        return s + "damage " + DurationDescriptor(duration);
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
