using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaitLimitStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.WAIT_LIMIT;
    public override float GetGameStat(BaseCharacter target) => target.WaitLimitModified;
    public override void SetGameStat(BaseCharacter target, float value) => target.ApplyWaitLimitModifier(value);
}