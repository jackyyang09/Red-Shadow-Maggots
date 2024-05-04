using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect Removal", menuName = "ScriptableObjects/Game Effects/Remove Effect", order = 1)]
public class EffectRemoval : BaseGameEffect
{
    public BaseGameEffect targetEffect;

    public override bool Activate(AppliedEffect effect)
    {
        effect.Target.EffectDictionary[targetEffect][0].Remove();

        return base.Activate(effect);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        var d = base.GetSkillDescription(targetMode, props);

        var effectName = targetEffect ? targetEffect.effectName : "NO TARGET";
        d = d.Replace("$TARGETEFFECT", "<u>" + effectName + "</u>");

        return d + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }
}