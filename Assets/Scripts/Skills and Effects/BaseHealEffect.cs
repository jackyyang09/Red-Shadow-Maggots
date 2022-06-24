using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal", order = 1)]
public class BaseHealEffect : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        target.Heal((int)GetEffectStrength(strength, customValues));
    }

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {

    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {

    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        int change = (int)GetEffectStrength(strength, customValues);

        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += "recovers ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "recover ";
                break;
        }

        s += change + " health ";

        return s + DurationDescriptor(duration);
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return (float)customValues[0];
            case EffectStrength.Weak:
                return 800;
            case EffectStrength.Small:
                return 1500;
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
