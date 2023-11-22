using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Revive Effect", menuName = "ScriptableObjects/Game Effects/On Death/Revive", order = 1)]
public class ReviveEffect : BaseOnDeathEffect
{
    public override bool IncludesExplainer => true;
    public override string ExplainerName => "Revive";
    public override string ExplainerDescription =>
        "When " + Keywords.Short.HEALTH + " reaches 0, " +
        "prevents death and regain " + Keywords.Short.HEALTH + ".";

    public GameStatValue value;

    public override void OnDeath(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        target.Heal(value.SolveValue(strength, user));
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        var target = TargetModeDescriptor(targetMode);

        string d = "";

        if (target == "")
        {
            d += "Receive a ";
        }
        else
        {
            d += target + "receive a ";
        }

        d += "<u>" + ExplainerName + "</u> status";

        d += " that recovers " + Keywords.Short.HEALTH + " equal to ";

        d += value.GetDescription(props.strength) + " " + 
            DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);

        return d;
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        return ExplainerDescription.Trim('.') + " equal to " + value.GetDescription(strength);
    }
}