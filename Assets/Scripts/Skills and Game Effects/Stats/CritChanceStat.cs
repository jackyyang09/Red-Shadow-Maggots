using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CRIT RATE", menuName = "ScriptableObjects/Game Stats/CRIT RATE", order = 1)]
public class CritChanceStat : BaseGameStat
{
    public override string Name => Keywords.Short.CRIT_CHANCE;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        if (!target.IsEnemy(out EnemyCharacter e))
        {
            target.ApplyCritChanceModifier(value);
        }
        else
        {
            if (value > 0) e.IncreaseChargeLevel();
            else e.DecreaseChargeLevel();
        }
    }

    public override float GetGameStat(BaseCharacter target) => target.CritChanceModified;
}