using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CRIT DMG", menuName = "ScriptableObjects/Game Stats/CRIT DMG", order = 1)]
public class CritDamageStat : BaseGameStat
{
    public override string Name => Keywords.Short.CRIT_DAMAGE;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyCritDamageModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.CritDamageModified;
}
