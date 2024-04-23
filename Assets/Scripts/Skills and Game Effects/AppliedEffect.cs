using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An instance of a GameEffect to be attached to an instanced Character
/// </summary>
public class AppliedEffect
{
    public BaseCharacter Caster => targetProps.Caster;
    public BaseCharacter Target => targetProps.Targets[0];
    public List<BaseCharacter> extraTargets = new List<BaseCharacter>();
    public TargetMode TargetMode => targetProps.TargetMode;
    public TargetProps targetProps;

    public BaseEffectTarget effectTarget;

    public BaseGameEffect referenceEffect;

    IStackableEffect stackEffect;
    public bool HasStacks => stackEffect != null;

    int maxStacks;
    int stacks;
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
                stacks = value;
                OnStacksChanged();
            }
        }
    }

    public EffectProperties.OldValue[] values;
    public ValueGroup valueGroup;
    public int startingTurns;
    public int remainingTurns;
    public int remainingActivations;
    public string description;
    public List<float> cachedValues = new List<float>();
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
                effectValues = values,
                maxStacks = maxStacks
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
        extraTargets = new List<BaseCharacter>(targets.Targets);
        extraTargets.RemoveAt(0);

        referenceEffect = props.effect;
        stackEffect = referenceEffect as IStackableEffect;

        remainingTurns = props.effectDuration;
        startingTurns = props.effectDuration;
        remainingActivations = props.activationLimit;
        // Internal variable rather than Property is used to prevent invokation of event
        stacks = props.stacks;
        maxStacks = props.maxStacks;
        values = props.effectValues;
    }

    public void Apply()
    {
        Activate();

        if (HasStacks) OnStacksChanged();
        else
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
        var active = referenceEffect.Activate(this);
        if (active && remainingActivations > 0)
        {
            remainingActivations--;
            if (remainingActivations == 0)
            {
                Target.RemoveEffect(this);
                active = false;
            }
        }
        RefreshDescription();
        return active;
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

    void OnStacksChanged()
    {
        var s = referenceEffect as IStackableEffect;
        s.OnStacksChanged(this);
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
            if (remainingActivations > 0)
            {
                remainingActivations--;
                if (remainingActivations == 0)
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