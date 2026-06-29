using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HealInStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.HEAL_RECEIVED;

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyHealInModifier(value);
    }

    public override float GetGameStat(BaseCharacter target) => target.HealInModifier;
}
