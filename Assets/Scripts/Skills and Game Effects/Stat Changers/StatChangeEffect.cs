using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat Change Effect", menuName = "ScriptableObjects/Game Effects/Stat Change Effect", order = 1)]
public class StatChangeEffect : BaseGameEffect
{
    [SerializeReference, SubclassSelector] public BaseGameStat stat;

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count == 0)
        {
            var value = effect.value.GetValue(effect.targetProps);
            stat.SetGameStat(effect.Target, value);
            effect.cachedValues.Add(new CachedValue { Value = value, Type = effect.value.ValueType });
        }
        else
        {
            var amount = effect.cachedValues[0];
            stat.SetGameStat(effect.Target, amount.Value);
        }

        return base.Activate(effect);
    }

    public override void OnExpire(AppliedEffect effect)
    {
        stat.SetGameStat(effect.Target, -effect.cachedValues[0].Value);

        base.OnExpire(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        var d = base.GetEffectDescription(effect);

        if (stat != null)
        {
            d = d.Replace("$STAT", stat.Name);
        }

        if (effect.value != null)
        {
            d = d.Replace("$VALUE", effect.cachedValues[0].Value.FormatTo(stat.ValueType));
        }

        d = effect.value.ProcessSkillDescription(effect.effectTarget, d);

        return d;
    }

    public override string GetSkillDescription(EffectGroup eg)
    {
        var d = base.GetSkillDescription(eg);

        var key = "$STAT";
        var statName = stat != null ? stat.Name : "NO STAT";
        d = d.Replace(key, statName);

        //if (props.value != null)
        //{
        //    d = props.value.ProcessSkillDescription(d);
        //}

        return d;
    }
}