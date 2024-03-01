using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Facade;

/// <summary>
/// Deprecated
/// </summary>
[CreateAssetMenu(fileName = "New Value", menuName = "ScriptableObjects/Game Stats/Value", order = 1)]
public class GameStatValue : ScriptableObject
{
    public enum StatTarget
    {
        Caster,
        Target,
        AllAllies,
        AllEnemies
    }

    public bool DontShowAsPercentage = false;
    public float Constant;
    public BaseGameStat Stat;
    public TargetMode Target;

    public float SolveValue(float value, BaseCharacter caster = null, BaseCharacter target = null)
    {
        float result = 0;

        if (Stat)
        {
            List<BaseCharacter> targets = new List<BaseCharacter>();
            switch (Target)
            {
                case TargetMode.None:
                    result = 1;
                    break;
                case TargetMode.Self:
                    targets.Add(caster);
                    break;
                case TargetMode.OneAlly:
                case TargetMode.OneEnemy:
                    targets.Add(target);
                    break;
                case TargetMode.AllAllies:
                    targets = battleSystem.PlayerCharacters.Where(t => t != null).ToList<BaseCharacter>();
                    break;
                case TargetMode.AllEnemies:
                    targets = enemyController.Enemies.Where(t => t != null).ToList<BaseCharacter>();
                    break;
            }

            foreach (var t in targets)
            {
                result += Stat.GetGameStat(t);
            }

            result *= value;

            result += Constant;
        }
        else
        {
            result = value;
        }

        return result;
    }

    //public string GetDescription(EffectStrength strength, BaseGameStat affectedStat)
    //{
    //    string description = "";
    //    
    //    var v = Mathf.Abs(GetStrength(strength));
    //
    //    // Likely percentage
    //    if (v <= 1)
    //    {
    //        description = (v * 100).ToString();
    //        if (!DontShowAsPercentage) description += "%";
    //    }
    //
    //    if (affectedStat != Stat)
    //    {
    //        if (Stat)
    //        {
    //            description += " of";
    //            switch (Target)
    //            {
    //                case TargetMode.None:
    //                case TargetMode.Self:
    //                    description += " the Caster's ";
    //                    break;
    //                case TargetMode.OneAlly:
    //                case TargetMode.OneEnemy:
    //                case TargetMode.AllAllies:
    //                case TargetMode.AllEnemies:
    //                    description += " your ";
    //                    break;
    //            }
    //
    //            description += Stat.Name;
    //        }
    //    }
    //
    //    if (Constant > 0)
    //    {
    //        description += " plus " + Constant;
    //    }
    //
    //    return description;
    //}
}