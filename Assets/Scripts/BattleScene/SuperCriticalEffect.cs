using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Facade;

public abstract class SuperCriticalEffect : MonoBehaviour
{
    [SerializeField] protected AnimationHelper animHelper = null;
    protected BaseCharacter baseCharacter { get { return animHelper.BaseCharacter; } }

    public virtual void DealSuperCritDamage()
    {

    }

    public virtual void BeginSuperCritEffect()
    {

    }

    public virtual void FinishSuperCritEffect()
    {
        GlobalEvents.OnCharacterFinishSuperCritical?.Invoke(baseCharacter);
        effectsApplied = 0;
    }

    int effectsApplied = 0;

    public void ApplyNextSuperCritEffect()
    {
        SkillObject superCrit = baseCharacter.Reference.superCritical;
        var effect = superCrit.gameEffects[effectsApplied];
        List<BaseCharacter> targets = new List<BaseCharacter>();
        var targetMode = effect.targetOverride == TargetMode.None ? superCrit.targetMode : effect.targetOverride;
        switch (targetMode)
        {
            case TargetMode.None:
            case TargetMode.Self:
                targets.Add(baseCharacter);
                break;
            case TargetMode.OneAlly:
                //target = baseCharacter;
                break;
            case TargetMode.OneEnemy:
                targets.Add(battleSystem.ActiveEnemy);
                break;
            case TargetMode.AllAllies:
                targets.AddRange(battleSystem.PlayerCharacters);
                break;
            case TargetMode.AllEnemies:
                targets.AddRange(enemyController.Enemies);
                break;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            BaseCharacter.ApplyEffectToCharacter(effect, targets[i], targetMode);
        }

        GlobalEvents.OnGameEffectApplied?.Invoke(superCrit.gameEffects[effectsApplied].effect);
        effectsApplied++;
    }
}