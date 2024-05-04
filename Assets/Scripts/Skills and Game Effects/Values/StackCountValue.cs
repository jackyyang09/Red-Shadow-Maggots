using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inherits from StatScaledValue to allow for 
/// "For each stack of X, increase STAT by VALUE% of STAT
/// </summary>
public class StackCountValue : BaseEffectValue
{
    public BaseGameEffect StackEffect;
    [SerializeReference] public BaseEffectTarget StackSource;

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

    public override string ProcessSkillDescription(string description, int index)
    {
        description = base.ProcessSkillDescription(description, index);

        var stackName = StackEffect ? StackEffect.effectName : "NO STACK";
        var key = "$STACK" + index;

        description = description.Replace(key, "<u>" + stackName + "</u>");

        return description;
    }
}