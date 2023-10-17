using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "Pee Poison", menuName = "ScriptableObjects/Game Effects/Pee Poison", order = 1)]
public class PeePoison : DamagePerTurnEffect
{
    public override bool IncludesExplainer => true;
    public override string EffectName => "Peed";
    public override string EffectDescription => 
        "Urinate every turn, losing " + Keywords.Short.HEALTH + 
        " proportional to your " + Keywords.Short.CURRENT_HEALTH + ".";

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float change = (float)GetEffectStrength(strength, customValues);

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
                s += " <u>Pee</u> ";
                break;
        }

        s += "at " + (int)(change * 100) + "% strength ";

        return s + DurationDescriptor(duration);
    }
}