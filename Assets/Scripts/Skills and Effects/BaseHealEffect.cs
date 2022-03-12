using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal", order = 1)]
public class BaseHealEffect : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        target.Heal((float)GetEffectStrength(strength, customValues));
    }

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {

    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {

    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        return "Recover " + GetEffectStrength(strength, customValues) + " health";
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return customValues[0];
            case EffectStrength.Weak:
                return 800f;
            case EffectStrength.Small:
                return 1500f;
            case EffectStrength.Medium:
                return 2000;
            case EffectStrength.Large:
                return 3000;
            case EffectStrength.EX:
                return 5000;
        }
        return 0;
    }
}
