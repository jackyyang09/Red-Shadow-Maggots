using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Current Health", menuName = "ScriptableObjects/Game Stats/Current Health", order = 1)]
public class CurrentHealthStat : BaseGameStat
{
    public override string Name => Keywords.Short.CURRENT_HEALTH;
    public override float GetGameStat(BaseCharacter target) => target.CurrentHealth;
}