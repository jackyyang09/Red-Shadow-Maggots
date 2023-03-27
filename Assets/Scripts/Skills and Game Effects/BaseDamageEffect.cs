using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Effect", menuName = "ScriptableObjects/Game Effects/Take Damage", order = 1)]
public class BaseDamageEffect : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float change = (float)GetEffectStrength(strength, customValues);

        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += "loses ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "lose ";
                break;
        }

        s += change + " health ";

        return s;
    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {

    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return customValues[0];
            case EffectStrength.Weak:
                return 200f;
            case EffectStrength.Small:
                return 300f;
            case EffectStrength.Medium:
                return 500f;
            case EffectStrength.Large:
                return 1000f;
            case EffectStrength.EX:
                return 3000f;
        }
        return 0;
    }
}
