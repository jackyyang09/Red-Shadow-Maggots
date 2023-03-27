using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(DodgeEffect), menuName = "ScriptableObjects/Game Effects/Dodge Effect", order = 1)]
public class DodgeEffect : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        target.IsDodging = true;
    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        target.IsDodging = false;
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        string d = "";
        if (targetMode == TargetMode.Self)
        {
            return "Avoids an attack " + DurationDescriptor(duration);;
        }
        else
        {
            switch (targetMode)
            {
                case TargetMode.OneAlly:
                    d = "Target ally avoids";
                    break;
                case TargetMode.AllAllies:
                    d = "All allies avoid";
                    break;
            }
        }
        return d += " an attack " + DurationDescriptor(duration);;
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        return null;
    }

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }
}