using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Effect", menuName = "ScriptableObjects/Game Effects/Take Damage", order = 1)]
public class BaseDamageEffect : BaseGameEffect
{
    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float change = (float)GetEffectStrength(strength, customValues);

        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.Self:
                s += "Lose ";
                break;
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += "loses ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "lose ";
                break;
        }

        s += change + "% health ";

        return s;
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return customValues[0];
            case EffectStrength.Weak:
                return 0.01f;
            case EffectStrength.Small:
                return 0.025f;
            case EffectStrength.Medium:
                return 0.05f;
            case EffectStrength.Large:
                return 0.075f;
            case EffectStrength.EX:
                return 0.1f;
        }
        return 0;
    }
}
