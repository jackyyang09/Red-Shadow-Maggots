using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatScaledValue : BaseEffectValue
{
    public float Multiplier;
    public BaseGameStat Stat;
    [SerializeReference] public BaseEffectTarget StatSource;

    public override float GetValue(TargetProps targetProps)
    {
        var targets = StatSource.GetTargets(targetProps.Caster, targetProps.Targets[0]);

        var v = Multiplier * Stat.GetGameStat(targets[0]);
        return v + base.GetValue(targetProps);
    }

    public override string ProcessSkillDescription(string description, int index)
    {
        var statName = Stat ? Stat.Name : "NO STAT";
        description = base.ProcessSkillDescription(description, index);

        var key = "$STAT" + index;

        description = description.Replace(key, statName);

        key = "$MULTIPLIER" + index;

        description = description.Replace(key, Multiplier.FormatPercentage());

        return description;
    }

    public override string GetEffectDescription(TargetProps targetProps)
    {
        var value = GetValue(targetProps);

        return value.FormatTo(ValueType);
    }
}