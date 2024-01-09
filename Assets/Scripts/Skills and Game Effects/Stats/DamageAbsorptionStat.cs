using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RSMConstants;

[CreateAssetMenu(fileName = "DMG ABSORPTION", menuName = "ScriptableObjects/Game Stats/DMG ABSORPTION", order = 1)]
public class DamageAbsorptionStat : BaseGameStat
{
    public override string Name => Keywords.Short.DAMAGE_ABSORPTION;

    public override float GetGameStat(BaseCharacter target)
    {
        return target.DamageAbsorptionModifier;
    }

    public override void SetGameStat(BaseCharacter target, float value)
    {
        target.ApplyDamageAbsorptionModifier(value);
    }
}