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
        superCritBuffsApplied = 0;
    }

    int superCritBuffsApplied = 0;
    public void ApplyNextSuperCritBuff()
    {
        SkillObject superCrit = baseCharacter.Reference.superCritical;
        baseCharacter.ApplyEffectToCharacter(superCrit.gameEffects[superCritBuffsApplied], baseCharacter);
        GlobalEvents.OnGameEffectApplied?.Invoke(superCrit.gameEffects[superCritBuffsApplied].effect);
        superCritBuffsApplied++;
    }
}