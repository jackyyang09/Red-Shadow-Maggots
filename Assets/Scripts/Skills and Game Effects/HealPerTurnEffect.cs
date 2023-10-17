using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal Per Turn", order = 1)]
public class HealPerTurnEffect : BaseHealEffect
{
    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void Tick(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        base.Tick(user, target, strength, customValues);
        base.Activate(user, target, strength, customValues);
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        var s = base.GetEffectDescription(targetMode, strength, customValues, duration);

        return s + "per turn " + DurationDescriptor(duration);
    }
}