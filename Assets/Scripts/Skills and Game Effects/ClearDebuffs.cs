using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Clear Debuffs", menuName = "ScriptableObjects/Game Effects/Clear Debuffs", order = 1)]
public class ClearDebuffs : BaseGameEffect
{
    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        List<BaseGameEffect> keys = new List<BaseGameEffect>();

        foreach (var effect in target.AppliedEffects)
        {
            if (effect.Key.effectType == EffectType.Debuff)
            {
                keys.Add(effect.Key);
            }
        }

        for (int i = 0; i < keys.Count; i++)
        {
            target.RemoveAllEffectsOfType(keys[i]);
        }
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        string desc = "Removes all debuffs from ";
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

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        return null;
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override void Tick(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }
}
