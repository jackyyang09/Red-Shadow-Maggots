using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetMode
{
    None,
    OneAlly,
    OneEnemy,
    AllAllies,
    AllEnemies,
    Self
}

public class TargetProps
{
    public BaseCharacter Caster;
    public BaseCharacter[] Targets;

    public TargetProps ShallowCopy()
    {
        return MemberwiseClone() as TargetProps;
    }
}

public enum ValueType
{
    [Tooltip("Multiplied by 100 w/ a `%`, always displays 1 decimal place " +
        "\nex. 0.0%, 99.9%")]
    Percentage,
    [Tooltip("Treated as is inputted")]
    Value,
    [Tooltip("Multiplied by 100 and rounded to lowest int " +
        "\nex. 0.999 -> 99")]
    Decimal
}

[System.Serializable]
public class EffectProperties
{
    public BaseGameEffect effect;
    [SerializeReference, SubclassSelector] public BaseEffectValue value;
    public int effectDuration = -1;
    public int activationLimit;
    public int stacks;
    public int maxStacks = -1;

    /// <summary>
    /// Shallow-Copy
    /// </summary>
    /// <returns></returns>
    public EffectProperties Copy()
    {
        var clone = (EffectProperties)MemberwiseClone();

        if (value != null) clone.value = value.Clone(); 

        return clone;
    }
}