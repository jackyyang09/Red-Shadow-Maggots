using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static RSMConstants.Keywords.Short;

public class BaseAbilityObject : ScriptableObject
{
    public string abilityName;
    public int coolDown;
    [SerializeReference, SubclassSelector] public SkillCondition condition;
    public Sprite sprite;
    public TargetMode targetMode;
    [SerializeReference, SubclassSelector] public EffectGroup[] effects = new EffectGroup[0];

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

            if (ge.effectProps != null)
            {
                if (ge.effectProps.effect != null)
                {
                    d += ge.effectProps.effect.GetSkillDescription(ge).Trim() + " " +
                        BaseGameEffect.DurationAndActivationDescriptor(ge.effectProps.effectDuration, ge.effectProps.activationLimit);
                }
            }

            if (ge.attackProps != null)
            {
                var dmg = "Deal DMG to " + ge.effectTarget.Descriptor + " equal to $VALUE";
                if (ge.attackProps.TotalDamage != null)
                {
                    d += ge.attackProps.TotalDamage.ProcessSkillDescription(ge.effectTarget, dmg);
                }
            }

            description.Add(d);
        }

        if (description.Count == 0) return new[] { "" };

        return description.ToArray();
    }

    public string GetEffectDescription()
    {
        var d = "";

        Debug.LogError("UNIMPLEMENTED");

        //var damageEffects = effects.Where(e => e.damageProps.effect).ToList();
        //
        //for (int i = 0; i < damageEffects.Count; i++)
        //{
        //    var group = damageEffects[i];
        //
        //    if (!effects[i].damageProps.effect) continue;
        //
        //    if (i == 0) d += "Deals ";
        //    else d += "deals ";
        //    d += DAMAGE + " equal to ";
        //
        //    //var value = group.damageProps.effectValues[0];
        //    //
        //    //d += BaseGameEffect.EffectValueDescriptor(value, "your",
        //    //    (group.damageProps.effect as InstantDamageEffect).Stat) + "to ";
        //
        //    switch (group.targetOverride)
        //    {
        //        case TargetMode.OneEnemy:
        //            d += "An Enemy";
        //            break;
        //        case TargetMode.AllEnemies:
        //            d += "All Enemies";
        //            break;
        //    }
        //
        //    if (i + 1 < damageEffects.Count) d += " and ";
        //}

        if (d.Length > 0) return d + ". ";
        return d;
    }
}