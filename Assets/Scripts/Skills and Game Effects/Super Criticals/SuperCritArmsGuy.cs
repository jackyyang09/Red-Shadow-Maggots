using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCritArmsGuy : SuperCriticalEffect
{
    public override void DealSuperCritDamage()
    {
        base.DealSuperCritDamage();
    }

    public override void BeginSuperCritEffect()
    {
        BaseCharacter.IncomingDamage.QTEResult = QuickTimeBase.QTEResult.Perfect;
        BaseCharacter.IncomingDamage.ChargeLevel = 3;
        BaseCharacter.IncomingDamage.DamageNormalized = 1.5f;
    }
}