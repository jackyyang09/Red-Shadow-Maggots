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

    public override bool Activate(AppliedEffect effect)
    {
        return true; // No need to cache values
    }

    public override void Tick(AppliedEffect effect)
    {
        var value = effect.value.GetValue(effect.targetProps);
        ConsumeHealth(effect.Target.CurrentHealth * value, effect.Target);
    }
}