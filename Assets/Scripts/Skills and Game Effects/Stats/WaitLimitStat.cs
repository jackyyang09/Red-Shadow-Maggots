using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WAIT LIMIT", menuName = "ScriptableObjects/Game Stats/WAIT LIMIT", order = 1)]
public class WaitLimitStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.WAIT_LIMIT;
    public override float GetGameStat(BaseCharacter target) => target.WaitLimitModified;
    public override void SetGameStat(BaseCharacter target, float value) => target.ApplyWaitLimitModifier(value);
    public override ValueType ValueType => ValueType.Decimal;
}