using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CurrentHealthStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.CURRENT_HEALTH;
    public override float GetGameStat(BaseCharacter target) => target.CurrentHealth;
}