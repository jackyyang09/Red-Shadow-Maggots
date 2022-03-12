using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal Per Turn", order = 1)]
public class HealPerTurnEffect : BaseHealEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        base.Activate(target, strength, customValues);
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        float healAmount = 0;
        switch (strength)
        {
            case EffectStrength.Custom:
                healAmount = customValues[0];
                break;
            case EffectStrength.Weak:
                healAmount = 800f;
                break;
            case EffectStrength.Small:
                healAmount = 1500f;
                break;
            case EffectStrength.Medium:
                healAmount = 2000;
                break;
            case EffectStrength.Large:
                healAmount = 3000;
                break;
            case EffectStrength.EX:
                healAmount = 5000;
                break;
        }
        return "Recover " + healAmount + " health per turn";
    }
}