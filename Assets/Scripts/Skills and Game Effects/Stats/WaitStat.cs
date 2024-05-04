using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WAIT", menuName = "ScriptableObjects/Game Stats/WAIT", order = 1)]
public class WaitStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.WAIT;
    public override float GetGameStat(BaseCharacter target) => target.Wait;
    public override void SetGameStat(BaseCharacter target, float value) => target.ApplyWaitModifier(value);
    public override ValueType ValueType => ValueType.Decimal;
}