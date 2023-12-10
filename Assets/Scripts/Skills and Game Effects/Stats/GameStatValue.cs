using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;
using static Facade;

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

    public float Constant;
    public GameStatValue Value;
    public BaseGameStat Stat;
    public TargetMode Target;
    public float[] Table;
    public float GetStrength(EffectStrength strength) => Table[(int)strength - 1];

    private void Reset()
    {
        Table = new float[(int)EffectStrength.EX];
    }

    public float SolveValue(EffectStrength strength, BaseCharacter caster = null, BaseCharacter target = null)
    {
        float result = 0;

        if (Value)
        {

        }
        else if (Stat)
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

            result *= GetStrength(strength);

            result += Constant;
        }

        return result;
    }

    public string GetDescription(EffectStrength strength)
    {
        string description = "";
        
        var v = GetStrength(strength);

        // Likely percentage
        if (Mathf.Abs(v) <= 1)
        {
            description = v * 100 + "%";
        }

        if (Stat)
        {
            description += " of";
            switch (Target)
            {
                case TargetMode.None:
                case TargetMode.Self:
                    description += " the Caster's ";
                    break;
                case TargetMode.OneAlly:
                case TargetMode.OneEnemy:
                case TargetMode.AllAllies:
                case TargetMode.AllEnemies:
                    description += " your ";
                    break;
            }

            description += Stat.Name;
        }

        if (Constant > 0)
        {
            description += " plus " + Constant;
        }

        return description;
    }
}