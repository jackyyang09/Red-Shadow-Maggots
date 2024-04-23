using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static RSMConstants.Keywords.Short;

public class BaseAbilityObject : ScriptableObject
{
    public string abilityName;
    public int coolDown;
    [SerializeReference] public SkillCondition condition;
    public Sprite sprite;
    public TargetMode targetMode;
    [SerializeReference] public EffectGroup[] effects = new EffectGroup[0];
    public EffectEvents[] events = new EffectEvents[0];

    public string[] GetSkillDescriptions()
    {
        var description = new List<string>();

        foreach (var ge in effects)
        {
            var d = "";

            if (ge == null)
            {
                description.Add("None");
                continue;
            }

            var target = ge.targetOverride == TargetMode.None ? targetMode : ge.targetOverride;

            if (ge.damageProps.effect)
            {
                d = ge.damageProps.effect.GetSkillDescription(target, ge.damageProps).ToString();
            }
            
            if (ge.effectProps.effect)
            {
                d = ge.effectProps.effect.GetSkillDescription(target, ge.effectProps).ToString();
            }

            var repeatStyle = ge.appStyle as RepeatedApplication;
            if (repeatStyle != null)
            {
                d += "(Repeat " + repeatStyle.Repeats + " Time";
                if (repeatStyle.Repeats > 1) d += "s";
                d += ")";
            }

            description.Add(d);
        }

        if (description.Count == 0) return new[] { "" };

        return description.ToArray();
    }

    public string GetEffectDescription()
    {
        var d = "";

        var damageEffects = effects.Where(e => e.damageProps.effect).ToList();

        for (int i = 0; i < damageEffects.Count; i++)
        {
            var group = damageEffects[i];

            if (!effects[i].damageProps.effect) continue;

            if (i == 0) d += "Deals ";
            else d += "deals ";
            d += DAMAGE + " equal to ";

            var value = group.damageProps.effectValues[0];

            d += BaseGameEffect.EffectValueDescriptor(value, "your",
                (group.damageProps.effect as InstantDamageEffect).Stat) + "to ";

            switch (group.targetOverride)
            {
                case TargetMode.OneEnemy:
                    d += "An Enemy";
                    break;
                case TargetMode.AllEnemies:
                    d += "All Enemies";
                    break;
            }

            if (i + 1 < damageEffects.Count) d += " and ";
        }

        if (d.Length > 0) return d + ". ";
        return d;
    }
}