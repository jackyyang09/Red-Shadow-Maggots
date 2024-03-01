using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static RSMConstants.Keywords.Short;

[CreateAssetMenu(fileName = "New Super", menuName = "ScriptableObjects/SuperCrit Object", order = 1)]
public class SuperCritObject : BaseAbilityObject
{
    public BaseGameStat damageScaledStat;
    public EffectProperties[] damageEffects;

    public string GetEffectDescription()
    {
        var d = "";

        EffectProperties.EffectValue total = new EffectProperties.EffectValue();

        var singleOutliers = new List<EffectProperties>();
        var aoeOutliers = new List<EffectProperties>();
        singleOutliers = damageEffects.Where(e => e.targetOverride == TargetMode.OneEnemy).ToList();
        aoeOutliers = damageEffects.Where(e => e.targetOverride == TargetMode.AllEnemies).ToList();

        foreach (var de in singleOutliers)
        {
            if (!de.effect) continue;
            if (de.effectValues.Length == 0) continue;

            total.multiplier += de.effectValues[0].multiplier;
            total.flat += de.effectValues[0].flat;
        }

        if (singleOutliers.Count > 0)
        {
            d += "Deals " + DAMAGE + " equal to " + BaseGameEffect.EffectValueDescriptor(total, "your",
                (singleOutliers[0].effect as InstantDamageEffect).Stat) + "to an Enemy";

            if (aoeOutliers.Count > 0) d += " and deals ";
        }
        else
        {
            d += "Deals ";
        }

        total = new EffectProperties.EffectValue();

        foreach (var de in aoeOutliers)
        {
            if (!de.effect) continue;
            if (de.effectValues.Length == 0) continue;

            total.multiplier += de.effectValues[0].multiplier;
            total.flat += de.effectValues[0].flat;
        }

        if (aoeOutliers.Count > 0)
        {
            d += "damage equal to " + BaseGameEffect.EffectValueDescriptor(total, "your",
                    (aoeOutliers[0].effect as InstantDamageEffect).Stat) + "to All Enemies";
        }

        return d;
    }
}