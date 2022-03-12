using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Defence Buff", menuName = "ScriptableObjects/Game Effects/Defence", order = 1)]
public class BaseDefenceBuff : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyDefenseModifier(percentageChange);
    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyDefenseModifier(-percentageChange);
    }

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff)
            return "Receive " + percentageChange * 100 + "% More Damage";
        else
            return "Receive " + percentageChange * 100 + "% Less Damage";
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
