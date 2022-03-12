using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Per Turn Effect", menuName = "ScriptableObjects/Game Effects/Take Damage", order = 1)]
public class DamagePerTurnEffect : BaseDamageEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        DamageStruct damageStruct = new DamageStruct();
        damageStruct.damage = (float)GetEffectStrength(strength, customValues);
        damageStruct.effectivity = DamageEffectivess.Normal;
        target.TakeDamage(damageStruct);
    }

    public override void TickCustom(BaseCharacter target, List<object> values)
    {
        DamageStruct damageStruct = new DamageStruct();
        float damage = 0;
        for (int i = 0; i < values.Count; i++)
        {
            damage += (float)values[i];
        }
        damageStruct.damage = damage;
        damageStruct.effectivity = DamageEffectivess.Normal;
        target.TakeDamage(damageStruct);
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        return "Lose " + GetEffectStrength(strength, customValues) + " health every turn";
    }
}
