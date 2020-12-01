using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Defence Buff", menuName = "ScriptableObjects/Skills/Defence", order = 1)]
public class BaseDefenceBuff : BaseGameEffect
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

        target.ApplyDefenseModifier(percentageChange);
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

        target.ApplyDefenseModifier(-percentageChange);
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
            return "Receive " + percentageChange * 100 + "% More Damage";
        else
            return "Receive " + percentageChange * 100 + "% Less Damage";
    }
}
