using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Leniency Buff", menuName = "ScriptableObjects/Game Effects/Attack Leniency", order = 1)]
public class BaseAttackLeniency : BaseGameEffect
{
    public override bool Activate(AppliedEffect effect)
    {
        float percentageChange = (float)GetEffectStrength(effect.strength, effect.customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        effect.target.ApplyAttackLeniencyModifier(percentageChange);

        //PlayerCharacter t = target as PlayerCharacter;
        //if (t) // This is a player
        //{
        //    t.ApplyAttackLeniencyModifier(percentageChange);
        //}
        //else // Enemies don't have Attack Leniency
        //{
        //    Debug.LogError("TODO: Enemies don't have Attack Leniency!");
        //}

        return true;
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        if (effectType == EffectType.Debuff) percentageChange *= -1;

        PlayerCharacter t = target as PlayerCharacter;
        if (t) // This is a player
        {
            t.ApplyAttackLeniencyModifier(-percentageChange);
        }
        else // Enemies don't have Attack Leniency
        {
            Debug.LogError("TODO: Enemies don't have Attack Leniency!");
        }
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        float percentageChange = (float)GetEffectStrength(props.strength, props.customValues);

        string s = TargetModeDescriptor(targetMode);

        s += "Attack Window ";

        if (effectType == EffectType.Debuff)
            s += "reduced";
        else
            s += "increased";

        return s + " by " + percentageChange * 100 + "% " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return customValues[0];
            case EffectStrength.Weak:
                return 0.05f;
            case EffectStrength.Small:
                return 0.15f;
            case EffectStrength.Medium:
                return 0.3f;
            case EffectStrength.Large:
                return 0.45f;
            case EffectStrength.EX:
                return 1f;
        }
        return 0;
    }
}