using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LeftRightValueOp : BaseEffectValue
{
    [SerializeReference, SubclassSelector] public BaseEffectValue Left;
    [SerializeReference, SubclassSelector] public BaseEffectValue Right;

    public override string ProcessSkillDescription(BaseEffectTarget target, string description)
    {
        if (Left != null)
        {
            description = Left.ProcessSkillDescription(target, description);
        }

        if (Right != null)
        {
            description = Right.ProcessSkillDescription(target, description);
        }

        return description;
    }
}

[System.Serializable]
public class AdditionValueOp : LeftRightValueOp
{
    public override string Descriptor => Left.Descriptor + " + " + Right.Descriptor;

    public override float GetValue(TargetProps targetProps)
    {
        return Left.GetValue(targetProps) + Right.GetValue(targetProps);
    }

    public override BattleState.SerializedValue Serialize()
    {
        var l = Left.Serialize();
        var r = Right.Serialize();

        return new()
        {
            Type = nameof(AdditionValueOp),
            Values = new[] { ((int)ValueType).ToString() },
            NestedValues = new[] { l, r }
        };
    }
}

[System.Serializable]
public class MinusValueOp : LeftRightValueOp
{
    public override string Descriptor => Left.Descriptor + " - " + Right.Descriptor;

    public override float GetValue(TargetProps targetProps)
    {
        return Left.GetValue(targetProps) - Right.GetValue(targetProps);
    }

    public override BattleState.SerializedValue Serialize()
    {
        var l = Left.Serialize();
        var r = Right.Serialize();

        return new()
        {
            Type = nameof(MinusValueOp),
            Values = new[] { ((int)ValueType).ToString() },
            NestedValues = new[] { l, r }
        };
    }
}

[System.Serializable]
public class MultiplyValueOp : LeftRightValueOp
{
    public override string Descriptor 
    {
        get
        {
            if (Left != null && Right != null)
            {
                if (Left.ValueType == ValueType.Percentage)
                {
                    return Left.Descriptor + " of " + Right.Descriptor;
                }
                else
                {
                    return Left.Descriptor + " x " + Right.Descriptor;
                }
            }
            return "LEFT x RIGHT";
        }
    }

    public override float GetValue(TargetProps targetProps)
    {
        return Left.GetValue(targetProps) * Right.GetValue(targetProps);
    }

    public override BattleState.SerializedValue Serialize()
    {
        var l = Left.Serialize();
        var r = Right.Serialize();

        return new()
        {
            Type = nameof(MultiplyValueOp),
            Values = new[] { ((int)ValueType).ToString() },
            NestedValues = new[] { l, r }
        };
    }

    public override string ProcessSkillDescription(BaseEffectTarget target, string description)
    {
        var d = description;
        string l = "", r = "";
        if (Left != null) l = Left.Descriptor;
        if (Right != null) r = Right.Descriptor;
        description = description.Replace("$VALUE", Descriptor);

        return description;
    }
}