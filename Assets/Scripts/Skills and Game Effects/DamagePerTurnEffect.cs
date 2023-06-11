using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Per Turn Effect", menuName = "ScriptableObjects/Game Effects/Take Damage", order = 1)]
public class DamagePerTurnEffect : BaseDamageEffect
{
    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void Tick(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        DamageStruct damageStruct = new DamageStruct();
        damageStruct.percentage = 1;
        damageStruct.damage = (float)GetEffectStrength(strength, customValues) * target.MaxHealth;
        damageStruct.effectivity = DamageEffectivess.Normal;
        target.TakeDamage(damageStruct);
    }

    public override void TickCustom(BaseCharacter user, BaseCharacter target, List<object> values)
    {
        float damage = 0;
        for (int i = 0; i < values.Count; i++)
        {
            damage += (float)values[i] * target.MaxHealth;
        }

        DamageStruct damageStruct = new DamageStruct();
        damageStruct.percentage = 1;
        damageStruct.damage = damage;
        damageStruct.effectivity = DamageEffectivess.Normal;
        target.TakeDamage(damageStruct);
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float change = (float)GetEffectStrength(strength, customValues);

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

        s += (int)(change * 100) + "% health every turn ";

        return s + DurationDescriptor(duration);
    }
}
