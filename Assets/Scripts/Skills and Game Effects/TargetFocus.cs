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

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.Caster.IsEnemy(out EnemyCharacter e))
        {
            Debug.LogWarning("TARGET FOCUS ON THE ENEMY HASN'T BEEN IMPLEMENTED YET");
        }
        else
        {
            var player = effect.Target as PlayerCharacter;
            BattleSystem.Instance.ApplyTargetFocus(player);
        }
        
        return true;
    }

    public override void OnExpire(AppliedEffect effect)
    {
        if (effect.Target.IsEnemy(out EnemyCharacter e))
        {
            Debug.LogWarning("TARGET FOCUS ON THE ENEMY HASN'T BEEN IMPLEMENTED YET");
        }
        else
        {
            var player = effect.Target as PlayerCharacter;
            BattleSystem.Instance.RemoveTargetFocus(player);
        }
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        return ExplainerDescription;
    }
}