using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DefenseWindowStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.DEFENSE_WINDOW;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyDefenseLeniencyModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.DefenseLeniencyModified;
}