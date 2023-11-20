using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Target Focus Effect", menuName = "ScriptableObjects/Game Effects/Target Focus", order = 1)]
public class BaseTargetFocus : BaseGameEffect
{
    public override bool IncludesExplainer => true;
    public override string ExplainerName => "Taunt";
    public override string ExplainerDescription =>
        "Increases likelihood of being attacked.";

    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        var player = target as PlayerCharacter;
        BattleSystem.Instance.ApplyTargetFocus(player);
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        var player = target as PlayerCharacter;
        BattleSystem.Instance.RemoveTargetFocus(player);
    }

    public override void Tick(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        string s = "Apply <u>Taunt</u> ";

        switch (targetMode)
        {
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += TargetModeDescriptor(targetMode);
                break;
        }

        return s + DurationDescriptor(duration);
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        return null;
    }
}
