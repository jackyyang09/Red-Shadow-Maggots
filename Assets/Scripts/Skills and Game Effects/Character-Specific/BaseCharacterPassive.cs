using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacterPassive : MonoBehaviour
{
    protected BaseCharacter baseCharacter;

    protected void OnEnable()
    {
        baseCharacter = GetComponentInParent<BaseCharacter>();
        if (baseCharacter) Init();
    }

    protected void OnDisable()
    {
        if (baseCharacter) Cleanup();
    }

    protected abstract void Init();
    protected abstract void Cleanup();

    protected virtual void ApplyEffect(BaseGameEffect e, int s)
    {
        if (s == 0) return;
        EffectProperties props = new() 
        { 
            effect = e, effectDuration = -1, stacks = s 
        };
        TargetProps targetProps = new()
        {
            Caster = baseCharacter, Targets = new[] { baseCharacter },
            TargetMode = TargetMode.Self
        };
        BaseCharacter.ApplyEffectToCharacter(props, targetProps);
    }

    protected void ApplyEffect(EffectProperties props)
    {
        TargetProps targetProps = new()
        {
            Caster = baseCharacter,
            Targets = new[] { baseCharacter },
            TargetMode = TargetMode.Self
        };
        BaseCharacter.ApplyEffectToCharacter(props, targetProps);
    }

    protected void ApplyEffectToCharacter(BaseCharacter character, EffectProperties props)
    {
        TargetProps targetProps = new()
        {
            Caster = baseCharacter,
            Targets = new[] { character },
            TargetMode = TargetMode.None
        };
        BaseCharacter.ApplyEffectToCharacter(props, targetProps);
    }

    protected bool HasStacks(BaseGameEffect effect)
    {
        if (baseCharacter.EffectDictionary.ContainsKey(effect))
        {
            return baseCharacter.EffectDictionary[effect].Count > 0;
        }
        return false;
    }

    protected int GetStackCount(BaseGameEffect stackEffect)
    {
        if (!HasStacks(stackEffect)) return 0;
        return baseCharacter.EffectDictionary[stackEffect][0].Stacks;
    }

    protected void DecreaseStack(BaseGameEffect statusEffect, int amount = 1)
    {
        if (!HasStacks(statusEffect)) return;
        var ae = baseCharacter.EffectDictionary[statusEffect][0];
        baseCharacter.RemoveEffect(ae, amount);
    }

    protected void RemoveEffect(BaseGameEffect statusEffect)
    {
        if (!HasStacks(statusEffect)) return;
        var ae = baseCharacter.EffectDictionary[statusEffect][0];
        baseCharacter.RemoveEffect(ae, ae.Stacks);
    }

    protected bool DefenseQTESuccessful(DamageStruct damage)
    {
        bool qteSuccess = false;
        switch (BattleSystem.Instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                qteSuccess = !baseCharacter.IsPlayer() && damage.QTEResult != QuickTimeBase.QTEResult.Perfect;
                break;
            case BattlePhases.EnemyTurn:
                qteSuccess = baseCharacter.IsPlayer() && damage.QTEResult == QuickTimeBase.QTEResult.Perfect;
                break;
        }
        return qteSuccess;
    }
}
