using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CachedValue
{
    public float Value;
    public ValueType Type;
    public string String() => Value.FormatTo(Type);
}

/// <summary>
/// An instance of a GameEffect to be attached to an instanced Character
/// </summary>
public class AppliedEffect
{
    public BaseCharacter Caster => targetProps.Caster;
    public BaseCharacter Target => targetProps.Targets[0];
    public TargetProps targetProps;

    public BaseEffectTarget effectTarget;

    public BaseGameEffect referenceEffect;

    IStackableEffect stackEffect;
    public bool HasStacks => stackEffect != null;

    int maxStacks;
    // Set stacks here if we don't want to trigger OnStacksChanged
    public int stacks;
    public int Stacks
    {
        get => stacks;
        set
        {
            if (maxStacks > 0)
            {
                value = Mathf.Min(value, maxStacks);
            }

            if (value != stacks)
            {
                var prev = stacks;
                stacks = value;
                OnStacksChanged(prev);
            }
        }
    }

    public int startingTurns;
    public int remainingTurns;
    public int startingUses;
    public int remainingUses;
    public string description;
    public List<CachedValue> cachedValues = new List<CachedValue>();
    public BaseEffectValue value;
    public System.Action[] customCallbacks;
    public GameObject[] instantiatedObjects;

    /// <summary>
    /// Constructs a basic EffectProperties instance based off exising values.
    /// Cache this value when using
    /// </summary>
    public EffectProperties Properties
    {
        get
        {
            return new()
            {
                effect = referenceEffect,
                effectDuration = startingTurns,
                maxStacks = maxStacks,
            };
        }
    }

    public AppliedEffect(BaseGameEffect effect)
    {
        referenceEffect = effect;
        stackEffect = referenceEffect as IStackableEffect;
    }

    public AppliedEffect(TargetProps targets, EffectProperties props)
    {
        targetProps = targets;

        referenceEffect = props.effect;
        stackEffect = referenceEffect as IStackableEffect;

        remainingTurns = props.effectDuration;
        startingTurns = props.effectDuration;
        remainingUses = props.activationLimit;
        startingUses = props.activationLimit;
        // Internal variable rather than Property is used to prevent invokation of event
        stacks = props.stacks;
        maxStacks = props.maxStacks;

        value = props.value;
    }

    public void Apply()
    {
        Activate();

        //if (!HasStacks)
        {
            RefreshDescription();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>bool - True if still active</returns>
    public bool Activate()
    {
        return referenceEffect.Activate(this);
    }

    public void UseOnce()
    {
        if (startingUses <= 0) return;

        remainingUses--;
        if (remainingUses == 0)
        {
            Target.RemoveEffect(this);
        }
        RefreshDescription();
    }

    /// <summary>
    /// </summary>
    /// <param name="target"></param>
    /// <returns>Is effect still active?</returns>
    public void Tick()
    {
        remainingTurns--;
        referenceEffect.Tick(this);
        if (remainingTurns == 0)
        {
            Remove();
            return;
        }
        RefreshDescription();
    }

    /// <summary>
    /// Deprecated, is this even used?
    /// Unlike Tick, doesn't activate the effect when called
    /// </summary>
    /// <returns>Is effect still active?</returns>
    //public bool TickSilent()
    //{
    //    remainingTurns--;
    //    if (remainingTurns == 0)
    //    {
    //        referenceEffect.OnExpire(this);
    //        return false;
    //    }
    //    return true;
    //}

    public void ResetDuration()
    {
        remainingTurns = startingTurns;
        RefreshDescription();
    }

    void OnStacksChanged(int previous)
    {
        var s = referenceEffect as IStackableEffect;
        s.OnStacksChanged(this, previous);
        RefreshDescription();
    }

    void RefreshDescription()
    {
        description = referenceEffect.GetEffectDescription(this);
    }

    public void OnExpire()
    {
        referenceEffect.OnExpire(this);
    }

    public void OnDeath()
    {
        referenceEffect.OnDeath(this);
        if (referenceEffect.activateOnDeath)
        {
            if (remainingUses > 0)
            {
                remainingUses--;
                if (remainingUses == 0)
                {
                    Remove();
                }
            }
        }
    }

    public void Remove()
    {
        Target.RemoveEffect(this);
    }

    public class AppliedEffectComparer : IEqualityComparer<AppliedEffect>
    {
        public bool Equals(AppliedEffect x, AppliedEffect y)
        {
            return x.referenceEffect == y.referenceEffect;
        }

        public int GetHashCode(AppliedEffect obj)
        {
            return GameEffectLoader.Instance.GetEffectIndex(obj.referenceEffect);
        }
    }
}