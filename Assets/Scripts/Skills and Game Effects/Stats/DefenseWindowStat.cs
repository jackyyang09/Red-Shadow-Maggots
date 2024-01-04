using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DEF WINDOW", menuName = "ScriptableObjects/Game Stats/DEF WINDOW", order = 1)]
public class DefenseWindowStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.DEFENSE_WINDOW;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyDefenseLeniencyModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.DefenseLeniencyModified;
}
