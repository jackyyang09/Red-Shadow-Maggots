using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "New Stat Changer", menuName = "ScriptableObjects/Game Effects/Stat Changer", order = 1)]
public class StatChangeEffect : BaseGameEffect
{
    [System.Serializable]
    public class StatChangePair
    {
        public BaseGameStat targetStat;
        public GameStatValue value;
    }

    [Header("Stat Change Properties")]
    [SerializeField] protected List<StatChangePair> stats;

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count == 0)
        {
            foreach (var stat in stats)
            {
                var amount = stat.value.SolveValue(effect.strength, effect.caster, effect.target);

                stat.targetStat.SetGameStat(effect.target, amount);

                effect.cachedValues.Add(amount);
            }
        }
        else
        {
            for (int i = 0; i < stats.Count; i++)
            {
                var amount = effect.cachedValues[i];

                stats[i].targetStat.SetGameStat(effect.target, amount);
            }
        }
        
        return base.Activate(effect);
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        foreach (var stat in stats)
        {
            var amount = stat.value.SolveValue(strength, user, target);

            stat.targetStat.SetGameStat(target, -amount);
        }

        base.OnExpire(user, target, strength, customValues);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string description = TargetModeDescriptor(targetMode);

        for (int i = 0; i < stats.Count; i++)
        {
            if (i > 0)
            {
                description += ", ";
            }

            description += stats[i].targetStat.Name;

            if (stats[i].value.Table[0] >= 0)
            {
                description += " increased ";
            }
            else
            {
                description += " decreased ";
            }

            description += "by " + stats[i].value.GetDescription(props.strength, stats[i].value.Stat) + " " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
        }

        return description;
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        string description = "";

        for (int i = 0; i < stats.Count; i++)
        {
            if (i > 0)
            {
                description += ", ";
            }

            description += stats[i].targetStat.Name;

            if (stats[i].value.Table[0] >= 0)
            {
                description += " increased ";
            }
            else
            {
                description += " decreased ";
            }

            description += "by " + stats[i].value.GetDescription(strength, stats[i].value.Stat);
        }

        return description;
    }
}