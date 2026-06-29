using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class BaseGameStat : BaseEffectValue
{
    public virtual string Name => "DEFAULT STAT NAME";
    public abstract float GetGameStat(BaseCharacter target);
    public virtual void SetGameStat(BaseCharacter target, float value) { }

    public override string Descriptor => Name;

    public override float GetValue(TargetProps targetProps)
    {
        float total = 0;
        foreach (var t in targetProps.Targets)
        {
            total += GetGameStat(t);
        }
        return total;
    }

    public override string ProcessSkillDescription(BaseEffectTarget target, string description)
    {
        var i = description.IndexOf("$VALUE");

        if (i > 0)
        {
            i += 6;
            description = description.Insert(i, " of " + Name);
        }

        return description;
    }

    public override BattleState.SerializedValue Serialize()
    {
        return new() { Type = GetType().ToString() };
    }
}

public class StatModifier
{
    float value;
    public float Value => value;
    AppliedEffect parentEffect;
    public AppliedEffect ParentEffect => parentEffect;

    public StatModifier(float v, AppliedEffect effect)
    {
        value = v;
        parentEffect = effect;
    }

    public void Deduct(float v)
    {
        value -= v;
    }
}