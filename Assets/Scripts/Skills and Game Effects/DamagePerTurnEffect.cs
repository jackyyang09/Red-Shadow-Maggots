using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Per Turn Effect", menuName = "ScriptableObjects/Game Effects/Take Damage Per Turn", order = 1)]
public class DamagePerTurnEffect : BaseDamageEffect
{
    public override float TickAnimationTime => -1f;

    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void Tick(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        DamageStruct damageStruct = new DamageStruct();
        damageStruct.Percentage = 1;
        damageStruct.TrueDamage = (float)GetEffectStrength(strength, customValues) * target.MaxHealth;
        damageStruct.Effectivity = DamageEffectivess.Normal;
        damageStruct.QTEResult = QuickTimeBase.QTEResult.Early;
        target.ConsumeHealth(damageStruct);
    }

    public override void TickCustom(BaseCharacter user, BaseCharacter target, List<object> values)
    {
        float damage = 0;
        for (int i = 0; i < values.Count; i++)
        {
            damage += (float)values[i] * target.MaxHealth;
        }

        DamageStruct damageStruct = new DamageStruct();
        damageStruct.Percentage = 1;
        damageStruct.TrueDamage = damage;
        damageStruct.Effectivity = DamageEffectivess.Normal;
        target.TakeDamage(damageStruct);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        float change = (float)GetEffectStrength(props.strength, props.customValues);

        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.Self:
                s += "Lose ";
                break;
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += " loses ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += " lose ";
                break;
        }

        s += (int)(change * 100) + "% Max Health every turn ";

        return s + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }
}
