using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.ATTACK;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyAttackModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.AttackModified;
}