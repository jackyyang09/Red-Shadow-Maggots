using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DEF", menuName = "ScriptableObjects/Game Stats/DEF", order = 1)]
public class DefenseStat : BaseGameStat
{
    public override string Name => Keywords.Short.DEFENSE;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyDefenseModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.DefenseModified;
}