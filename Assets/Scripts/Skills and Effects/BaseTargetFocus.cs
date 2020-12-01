using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Target Focus Effect", menuName = "ScriptableObjects/Skills/Target Focus", order = 1)]
public class BaseTargetFocus : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        var player = target as PlayerCharacter;
        BattleSystem.instance.ApplyTargetFocus(player);
    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        var player = target as PlayerCharacter;
        BattleSystem.instance.RemoveTargetFocus(player);
    }

    public override void Tick()
    {
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        return "High Priority Target";
    }
}
