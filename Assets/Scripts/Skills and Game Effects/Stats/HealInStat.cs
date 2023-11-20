using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HEALING", menuName = "ScriptableObjects/Game Stats/HEALING", order = 1)]
public class HealInStat : BaseGameStat
{
    public override string Name => Keywords.Short.HEAL_RECEIVED;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyHealInModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.HealInModifier;
}
