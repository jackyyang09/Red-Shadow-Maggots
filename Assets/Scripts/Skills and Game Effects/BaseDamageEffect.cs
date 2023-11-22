using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Effect", menuName = "ScriptableObjects/Game Effects/Take Damage", order = 1)]
public class BaseDamageEffect : BaseGameEffect
{
    public GameStatValue value;

    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        base.Activate(user, target, strength, customValues);

        DamageStruct damageStruct = new DamageStruct();
        damageStruct.Percentage = 1;
        damageStruct.TrueDamage = (float)GetEffectStrength(strength, customValues) * target.MaxHealth;
        damageStruct.Effectivity = DamageEffectivess.Normal;
        target.ConsumeHealth(damageStruct);
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
                s += " lose ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += " lose ";
                break;
        }

        s += Keywords.Short.HEALTH + " equal to " + change * 100 + "% of your " + Keywords.Short.MAX_HEALTH;

        return s;
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return customValues[0];
            case EffectStrength.Weak:
                return 0.01f;
            case EffectStrength.Small:
                return 0.025f;
            case EffectStrength.Medium:
                return 0.05f;
            case EffectStrength.Large:
                return 0.075f;
            case EffectStrength.EX:
                return 0.1f;
        }
        return 0;
    }
}
