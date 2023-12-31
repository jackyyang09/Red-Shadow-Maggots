using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(DodgeEffect), menuName = "ScriptableObjects/Game Effects/Dodge Effect", order = 1)]
public class DodgeEffect : BaseGameEffect
{
    public override bool Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        target.IsDodging = true;
        // TODO: Re-applying dodge effect should remove other dodge instances
        Debug.LogWarning(nameof(DodgeEffect) + ": Re-applying dodge effect should remove other dodge instances?");
        return true;
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        target.IsDodging = false;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string d = "";
        if (targetMode == TargetMode.Self)
        {
            return "Avoids an attack " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
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
        return d += " an attack " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        return "Avoids an attack";
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        return null;
    }
}