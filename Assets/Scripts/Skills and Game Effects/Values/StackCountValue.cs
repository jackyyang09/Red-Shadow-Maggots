using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StackCountValue : BaseEffectValue
{
    public BaseGameEffect StackEffect;
    [SerializeReference, SubclassSelector] public BaseEffectTarget StackSource;
    public override string Descriptor 
    {
        get
        {
            var stackName = StackEffect ? StackEffect.effectName : "NO STACK";
            var source = StackSource != null ? StackSource.Descriptor : "NO SOURCE";
            return "stacks of <u>" + stackName + "</u> on " + source;
        }
    }

    public override float GetValue(TargetProps targetProps)
    {
        var targets = StackSource.GetTargets(targetProps.Caster, targetProps.Targets[0]);

        int stacks = 0;
        foreach (var t in targets)
        {
            if (t.EffectDictionary.ContainsKey(StackEffect))
            {
                var e = t.EffectDictionary[StackEffect][0];
                stacks += e.Stacks;
            }
        }

        return stacks;
    }

    public override string ProcessSkillDescription(BaseEffectTarget target, string description)
    {
        var stackName = StackEffect ? StackEffect.effectName : "NO STACK";
        description = "For each stack of <u>" + stackName + "</u> on " + target.Descriptor + ", " + description;

        return description;
    }

    public override BattleState.SerializedValue Serialize()
    {
        return new()
        {
            Type = nameof(StackCountValue),
            Values = new[]
            {
                ((int) ValueType).ToString(),
                GameEffectLoader.Instance.GetEffectIndex(StackEffect).ToString(),
                StackSource.GetType().ToString()
            }
        };
    }
}