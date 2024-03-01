using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Game Effects/Heal", order = 1)]
public class BaseHealEffect : BaseStatScaledEffect
{
    public override bool Activate(AppliedEffect effect)
    {
        var amount = GetValue(stat, effect.values[0], effect.caster);

        if (effect.cachedValues.Count == 0)
        {
            effect.cachedValues.Add(amount);
        }
        else
        {
            amount = effect.cachedValues[0];
        }

        effect.target.Heal(amount);

        return true;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.None:
            case TargetMode.Self:
                s += "Recover ";
                break;
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
            EffectValueDescriptor(props.effectValues[0], "your", stat);

        return s;
    }
}