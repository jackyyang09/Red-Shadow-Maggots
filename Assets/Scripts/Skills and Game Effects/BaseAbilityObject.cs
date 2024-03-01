using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAbilityObject : ScriptableObject
{
    public string abilityName;
    public Sprite sprite;
    public TargetMode targetMode;
    public EffectProperties[] gameEffects;

    public string[] GetSkillDescriptions()
    {
        var d = new List<string>();

        foreach (var ge in gameEffects)
        {
            if (!ge.effect) continue;

            var target = ge.targetOverride == TargetMode.None ? targetMode : ge.targetOverride;

            d.Add(ge.effect.GetSkillDescription(target, ge).ToString());
        }

        return d.ToArray();
    }
}