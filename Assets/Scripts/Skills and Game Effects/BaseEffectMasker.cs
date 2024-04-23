using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect Mask", menuName = "ScriptableObjects/Effect Mask", order = 1)]
public class BaseEffectMasker : BaseGameEffect
{
    [SerializeField] EffectType[] effectMask;

    public override bool Activate(AppliedEffect effect)
    {
        foreach (var e in effectMask)
        {
            effect.Target.ApplyEffectMask(e, effect);
        }

        return base.Activate(effect);
    }

    string GetBaseDescription()
    {
        string d = "Resist all ";

        for (int i = 0; i < effectMask.Length; i++)
        {
            if (i == effectMask.Length - 1 && effectMask.Length > 1)
            {
                d += "and ";
            }

            switch (effectMask[i])
            {
                case EffectType.Heal:
                case EffectType.Buff:
                case EffectType.Debuff:
                    d += effectMask[i];
                    break;
            }

            if (i + 1 < effectMask.Length)
            {
                d += ", ";
            }
            else
            {
                d += " ";
            }
        }

        d += "effects ";

        return d;
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        return GetBaseDescription();
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string d = TargetModeDescriptor(targetMode);

        var baseD = GetBaseDescription();

        switch (targetMode)
        {
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                baseD = baseD.FirstCharToLower();
                break;
        }

        d += baseD + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);

        return d + base.GetSkillDescription(targetMode, props);
    }
}