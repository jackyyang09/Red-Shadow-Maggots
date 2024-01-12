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
        ConsumeHealth(effect.target, effect.strength, effect.customValues);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        float change = (float)GetEffectStrength(props.strength, props.customValues);

        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.Self:
                s += "<u>Pee</u> ";
                break;
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "<u>Pee</u> ";
                break;
        }

        s += "at " + (int)(change * 100) + "% strength ";

        return s + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        var d = "Urinate every turn, losing " + RSMConstants.Keywords.Short.HEALTH +
            " equal to " + value.GetDescription(strength, value.Stat);
        return d;
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return customValues[0];
            case EffectStrength.Weak:
                return 0.025f;
            case EffectStrength.Small:
                return 0.05f;
            case EffectStrength.Medium:
                return 0.15f;
            case EffectStrength.Large:
                return 0.2f;
            case EffectStrength.EX:
                return 0.25f;
        }
        return 0;
    }
}