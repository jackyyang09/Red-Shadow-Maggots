using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritDamageStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.CRIT_DAMAGE;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyCritDamageModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.CritDamageModified;
}
