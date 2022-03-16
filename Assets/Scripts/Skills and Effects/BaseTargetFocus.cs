using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Target Focus Effect", menuName = "ScriptableObjects/Game Effects/Target Focus", order = 1)]
public class BaseTargetFocus : BaseGameEffect
{
    public override void Activate(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        var player = target as PlayerCharacter;
        BattleSystem.Instance.ApplyTargetFocus(player);
    }

    public override void OnExpire(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        var player = target as PlayerCharacter;
        BattleSystem.Instance.RemoveTargetFocus(player);
    }

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        string s = "Increases likelihood of ";

        switch (targetMode)
        {
            case TargetMode.OneAlly:
                s += "an Ally ";
                break;
            case TargetMode.OneEnemy:
                s += "an Enemy ";
                break;
        }
        return s + "being attacked " + DurationDescriptor(duration);
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        return null;
    }
}
