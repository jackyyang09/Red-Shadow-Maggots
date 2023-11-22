using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stat Changer", menuName = "ScriptableObjects/Game Effects/Stat Changer", order = 1)]
public class StatChangeEffect : BaseGameEffect
{
    [Header("Stat Change Properties")]
    [SerializeField] BaseGameStat targetStat;
    [SerializeField] GameStatValue value;

    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        var amount = value.SolveValue(strength, user, target);

        targetStat.SetGameStat(target, amount);

        base.Activate(user, target, strength, customValues);
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        var amount = value.SolveValue(strength, user, target);

        targetStat.SetGameStat(target, -amount);

        base.OnExpire(user, target, strength, customValues);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string description = TargetModeDescriptor(targetMode);

        description += targetStat.Name;

        if (value.Table[0] >= 0)
        {
            description += " increased ";
        }
        else
        {
            description += " decreased ";
        }

        description += "by " + value.GetDescription(props.strength) + " " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);

        return description;
    }
}