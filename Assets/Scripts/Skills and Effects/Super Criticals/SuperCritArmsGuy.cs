using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCritArmsGuy : SuperCriticalEffect
{
    public override void BeginSuperCritEffect()
    {
        BaseCharacter.IncomingDamage.qteResult = QuickTimeBase.QTEResult.Perfect;
        BaseCharacter.IncomingDamage.chargeLevel = 3;
        BaseCharacter.IncomingDamage.damageNormalized = 1.5f;
    }
}