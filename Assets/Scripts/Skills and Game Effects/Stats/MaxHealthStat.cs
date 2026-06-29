using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaxHealthStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.MAX_HEALTH;
    public override float GetGameStat(BaseCharacter target) => target.MaxHealth;
}