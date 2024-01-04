using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ATK WINDOW", menuName = "ScriptableObjects/Game Stats/ATK WINDOW", order = 1)]
public class AttackWindowStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.ATTACK_WINDOW;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyAttackLeniencyModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.AttackLeniencyModified;
}
