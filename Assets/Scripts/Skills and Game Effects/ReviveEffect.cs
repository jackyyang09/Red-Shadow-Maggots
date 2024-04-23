using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Revive Effect", menuName = "ScriptableObjects/Game Effects/On Death/Revive", order = 1)]
public class ReviveEffect : BaseStatScaledEffect
{
    public override bool IncludesExplainer => true;
    public override string ExplainerName => "Revive";
    public override string ExplainerDescription =>
        "When " + RSMConstants.Keywords.Short.HEALTH + " reaches 0, " +
        "prevents death and regain " + RSMConstants.Keywords.Short.HEALTH + ".";

    public override bool Activate(AppliedEffect effect)
    {
        effect.cachedValues.Clear();
        effect.cachedValues.Add(GetValue(stat, effect.values[0], effect.Target));

        return base.Activate(effect);
    }

    public override void OnDeath(AppliedEffect effect)
    {
        effect.Target.Heal(effect.cachedValues[0]);
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

        d += " that recovers " + RSMConstants.Keywords.Short.HEALTH + " equal to ";

        d += EffectValueDescriptor(props.effectValues[0], stat) +
            DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);

        return d;
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        string d = ExplainerDescription;
        var index = d.IndexOf(RSMConstants.Keywords.Short.HEALTH);
        return d.Insert(index, effect.cachedValues[0] + " ");
    }
}