using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WAIT", menuName = "ScriptableObjects/Game Stats/WAIT", order = 1)]
public class WaitStat : BaseGameStat
{
    public override string Name => Keywords.Short.WAIT;
    public override float GetGameStat(BaseCharacter target) => target.WaitTimer;
    public override void SetGameStat(BaseCharacter target, float value) => target.ApplyWaitModifier(value);
}
