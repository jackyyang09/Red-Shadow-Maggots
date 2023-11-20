using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MAX HP", menuName = "ScriptableObjects/Game Stats/MAX HP", order = 1)]
public class MaxHealthStat : BaseGameStat
{
    public override string Name => Keywords.Short.MAX_HEALTH;
    public override float GetGameStat(BaseCharacter target) => target.MaxHealth;
}