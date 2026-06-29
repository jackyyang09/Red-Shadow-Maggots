using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaitStat : BaseGameStat
{
    public override string Name => RSMConstants.Keywords.Short.WAIT;
    public override float GetGameStat(BaseCharacter target) => target.Wait;
    public override void SetGameStat(BaseCharacter target, float value) => target.ApplyWaitModifier(value);
}