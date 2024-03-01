using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "Pee Poison", menuName = "ScriptableObjects/Game Effects/Pee Poison", order = 1)]
public class PeePoison : DamagePerTurnEffect
{
    public override bool IncludesExplainer => true;
    public override string ExplainerName => "Peed";
    public override string ExplainerDescription => 
        "Urinate every turn, losing " + RSMConstants.Keywords.Short.HEALTH + 
        " proportional to your " + RSMConstants.Keywords.Short.CURRENT_HEALTH + ".";

    public override float TickAnimationTime => 0.5f;

    public override void Tick(AppliedEffect effect)
    {
        ConsumeHealth(effect.cachedValues[0], effect.target);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.Self:
                s += "<u>Pee</u> ";
                break;
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += "<u>Pees</u> ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "<u>Pee</u> ";
                break;
        }

        s += "at " + props.effectValues[0].multiplier.FormatPercentage() + " strength ";

        return s + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        var d = "Urinate every turn, losing " + RSMConstants.Keywords.Short.HEALTH +
            " equal to " + EffectValueDescriptor(effect.values[0], "your", stat);
        return d;
    }
}