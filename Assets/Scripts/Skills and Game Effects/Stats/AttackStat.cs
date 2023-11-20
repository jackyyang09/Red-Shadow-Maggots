using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ATK", menuName = "ScriptableObjects/Game Stats/ATK", order = 1)]
public class AttackStat : BaseGameStat
{
    public override string Name => Keywords.Short.ATTACK;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyAttackModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.AttackModified;
}
