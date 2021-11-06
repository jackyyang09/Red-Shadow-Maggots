using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Buff", menuName = "ScriptableObjects/Game Effects/Attack Damage", order = 1)]
public class BaseAttackBuff : BaseGameEffect
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
                percentageChange = 0.1f;
                break;
            case EffectStrength.Medium:
                percentageChange = 0.2f;
                break;
            case EffectStrength.Large:
                percentageChange = 0.3f;
                break;
            case EffectStrength.EX:
                percentageChange = 0.5f;
                break;
        }
        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyAttackModifier(percentageChange);
    }

    public override void Tick()
    {

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
                percentageChange = 0.1f;
                break;
            case EffectStrength.Medium:
                percentageChange = 0.2f;
                break;
            case EffectStrength.Large:
                percentageChange = 0.3f;
                break;
            case EffectStrength.EX:
                percentageChange = 0.5f;
                break;
        }
        if (effectType == EffectType.Debuff) percentageChange *= -1;

        target.ApplyAttackModifier(-percentageChange);
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
                percentageChange = 0.1f;
                break;
            case EffectStrength.Medium:
                percentageChange = 0.2f;
                break;
            case EffectStrength.Large:
                percentageChange = 0.3f;
                break;
            case EffectStrength.EX:
                percentageChange = 0.5f;
                break;
        }

        if (effectType == EffectType.Debuff)
            return "Attack Damage Reduced by " + percentageChange * 100 + "%";
        else
            return "Attack Damage Increased by " + percentageChange * 100 + "%";
    }
}