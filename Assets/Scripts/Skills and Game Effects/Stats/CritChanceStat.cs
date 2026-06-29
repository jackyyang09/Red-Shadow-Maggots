using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritChanceStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.CRIT_CHANCE;

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