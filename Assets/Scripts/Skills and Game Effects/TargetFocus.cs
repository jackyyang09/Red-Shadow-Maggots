using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

[CreateAssetMenu(fileName = "New Target Focus Effect", menuName = "ScriptableObjects/Game Effects/Target Focus", order = 1)]
public class TargetFocus : BaseGameEffect
{
    public override bool IncludesExplainer => true;
    public override string ExplainerName => "Taunt";
    public override string ExplainerDescription =>
        "Increases likelihood of being attacked.";

    public override bool Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        if (user.IsEnemy(out EnemyCharacter e))
        {
            Debug.LogWarning("TARGET FOCUS ON THE ENEMY HASN'T BEEN IMPLEMENTED YET");
        }
        else
        {
            var player = target as PlayerCharacter;
            BattleSystem.Instance.ApplyTargetFocus(player);
        }
        
        return true;
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        if (user.IsEnemy(out EnemyCharacter e))
        {
            Debug.LogWarning("TARGET FOCUS ON THE ENEMY HASN'T BEEN IMPLEMENTED YET");
        }
        else
        {
            var player = target as PlayerCharacter;
            BattleSystem.Instance.RemoveTargetFocus(player);
        }
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string s = "Apply <u>Taunt</u> to ";

        switch (targetMode)
        {
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += TargetModeDescriptor(targetMode);
                break;
        }

        return s + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        return ExplainerDescription;
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        return null;
    }
}
