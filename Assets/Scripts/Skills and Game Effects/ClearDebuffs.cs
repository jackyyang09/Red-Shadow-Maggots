using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Clear Debuffs", menuName = "ScriptableObjects/Game Effects/Clear Debuffs", order = 1)]
public class ClearDebuffs : BaseGameEffect
{
    public override bool Activate(AppliedEffect effect)
    {
        var effects = effect.Target.AppliedEffects.Where(e => e.referenceEffect.effectType == EffectType.Debuff).ToList();

        if (effects.Count == 0) return false;

        effects[0].Remove();

        return true;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string desc = "Removes 1 Debuff from ";
        switch (targetMode)
        {
            case TargetMode.None:
            case TargetMode.Self:
                desc += "Self";
                break;
            case TargetMode.OneAlly:
                desc += "Ally";
                break;
            case TargetMode.OneEnemy:
                desc += "Enemy";
                break;
            case TargetMode.AllAllies:
                desc += "All Allies";
                break;
            case TargetMode.AllEnemies:
                desc += "All Enemies";
                break;
        }
        return desc;
    }
}
