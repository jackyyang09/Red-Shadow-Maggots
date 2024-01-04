using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bushman Supplies", menuName = "ScriptableObjects/Character-Specific/Bushman Supplies", order = 1)]
public class BushmanSupplies : BaseGameEffect, IStackableEffect
{
    [Header("Tier 1 - Heal per Turn")]
    [SerializeField] int healRequirement;
    [SerializeField] float healAmount = 0.01f;

    [Header("Tier 3 - Crit Chance Buff")]
    [SerializeField] int critChanceRequirement;
    [SerializeField] float critChanceModifier = 0.3f;

    [Header("Tier 4 - Dodge")]

    bool critChanceChanged;

    public void OnStacksChanged(AppliedEffect effect)
    {
        if (effect.Stacks >= critChanceRequirement && !critChanceChanged)
        {
            critChanceChanged = true;
            effect.target.ApplyCritChanceModifier(critChanceModifier);
        }

        //if (effect.Stacks >= dodgeRequirement)
        //{
        //    EffectProperties props = new EffectProperties() { effect = dodgeEffect, effectDuration = 1, activationLimit = 1 };
        //    ApplyEffect(props);
        //    SuppliesEffect.description += ". ";
        //}
    }

    public override void Tick(AppliedEffect effect)
    {
        if (effect.Stacks < healRequirement) return;

        effect.target.Heal(effect.caster.MaxHealth * effect.Stacks * healAmount);

        base.Tick(effect);
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        string d = "Gain effects depending on stack count:\n" +
            "<indent=2.5%>" +
            "-<indent=5%>(Stacks >= 1) For each stack, heal " + RSMConstants.Keywords.Short.HEALTH + " equal to " +
            healAmount.FormatPercentage() + " of your " + RSMConstants.Keywords.Short.MAX_HEALTH + ".</indent>\n" +
            "-<indent=5%>(Stacks >= 10) Gain " + critChanceModifier.FormatPercentage() + " Crit Chance.</indent>\n" +
            "-<indent=5%>(Stacks >= 15) Every turn, apply Dodge (1 Time, 1 Turn).</indent>";

        return d;
    }
}