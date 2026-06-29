using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackWindowStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.ATTACK_WINDOW;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyAttackLeniencyModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.AttackLeniencyModified;
}
