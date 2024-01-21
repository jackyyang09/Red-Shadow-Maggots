using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal", order = 1)]
public class BaseHealEffect : BaseGameEffect
{
    [SerializeField] protected GameStatValue value;

    public override bool Activate(AppliedEffect effect)
    {
        var amount = value.SolveValue(effect.strength, effect.caster, effect.target);

        //var amount = (float)GetEffectStrength(strength, customValues) * user.MaxHealth;

        effect.target.Heal(amount);

        return true;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        var change = (int)((float)GetEffectStrength(props.strength, props.customValues) * 100);

        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += "recovers ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "recover ";
                break;
        }

        s += RSMConstants.Keywords.Short.HEALTH + " equal to " +
            change + "% of your " + RSMConstants.Keywords.Short.MAX_HEALTH;

        return s;
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                if (customValues.Length == 0)
                {
                    return 0f;
                }
                else return (float)customValues[0];
            case EffectStrength.Weak:
                return 0.05f;
            case EffectStrength.Small:
                return 0.15f;
            case EffectStrength.Medium:
                return 0.3f;
            case EffectStrength.Large:
                return 0.6f;
            case EffectStrength.EX:
                return 1f;
        }
        return 0;
    }
}
