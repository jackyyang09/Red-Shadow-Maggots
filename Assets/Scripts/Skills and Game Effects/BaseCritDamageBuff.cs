using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crit Damage Effect", menuName = "ScriptableObjects/Game Effects/Crit Damage", order = 1)]
public class BaseCritDamageBuff : BaseGameEffect
{
    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyCritDamageModifier(percentageChange);
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyCritDamageModifier(-percentageChange);
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        string s = TargetModeDescriptor(targetMode);

        s += "Crit Damage ";

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
