using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Facade;

/// <summary>
/// Deprecated
/// </summary>
public abstract class SuperCriticalEffect : MonoBehaviour
{
    [SerializeField] protected AnimationHelper animHelper = null;
    protected BaseCharacter baseCharacter { get { return animHelper.BaseCharacter; } }

    public virtual void DealSuperCritDamage()
    {

    }

    public virtual void BeginSuperCritEffect(int startIndex)
    {
        effectsApplied = startIndex;
    }

    public virtual void FinishSuperCritEffect()
    {
        GlobalEvents.OnCharacterFinishSuperCritical?.Invoke(baseCharacter);
    }

    int effectsApplied;

    public void ApplyNextSuperCritEffect()
    {
        var superCrit = baseCharacter.Reference.superCritical;
        if (effectsApplied >= superCrit.effects.Length)
        {
            Debug.LogWarning(nameof(SuperCriticalEffect) + " - " + gameObject.name + ": " +
                "Tried to apply next super critical effect, but all effects have been applied!");
            return;
        }

        var effect = superCrit.effects[effectsApplied];
        var targets = superCrit.effects[effectsApplied].effectTarget.GetTargets(baseCharacter, battleSystem.OpposingCharacter);

        foreach (var t in targets)
        {
            effect.appStyle.Apply(effect, baseCharacter, t);
        }

        effectsApplied++;
    }
}