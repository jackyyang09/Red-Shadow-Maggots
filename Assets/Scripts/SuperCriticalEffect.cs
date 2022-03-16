using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class SuperCriticalEffect : MonoBehaviour
{
    [SerializeField] protected AnimationHelper animHelper = null;
    protected BaseCharacter baseCharacter { get { return animHelper.BaseCharacter; } }

    public abstract void BeginSuperCritEffect();

    public virtual void FinishSuperCritEffect()
    {
        GlobalEvents.OnCharacterFinishSuperCritical?.Invoke(baseCharacter);
        effectsApplied = 0;
    }

    int effectsApplied = 0;
    public void ApplyNextSuperCritBuff()
    {
        SkillObject superCrit = baseCharacter.Reference.superCritical;
        baseCharacter.ApplyEffectToCharacter(superCrit.gameEffects[effectsApplied], baseCharacter, TargetMode.Self);
        GlobalEvents.OnGameEffectApplied?.Invoke(superCrit.gameEffects[effectsApplied].effect);
        effectsApplied++;
    }
}