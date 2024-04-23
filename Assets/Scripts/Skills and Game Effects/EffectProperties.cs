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
    public TargetMode TargetMode;

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
    [System.Serializable]
    public class OldValue
    {
        [Tooltip("This value will be multiplied with a different value")]
        public float multiplier;
        [Tooltip("Any values to be added on")]
        public float flat;
        [Tooltip("Defines how the flat value is displayed in Skill Descriptions")]
        public ValueType flatType;
        [Tooltip("Defines how the value change is displayed in the Effect Description" +
            "\nex. ATK increased by 20 vs ATK increased by 20%")]
        public ValueType deltaType;

        public OldValue Copy()
        {
            return MemberwiseClone() as OldValue;
        }

        public static OldValue operator *(OldValue e, int stacks)
        {
            var c = e.Copy();
            c.multiplier *= stacks;
            c.flat *= stacks;
            return c;
        }

        public static OldValue operator *(OldValue e, float stacks)
        {
            var c = e.Copy();
            c.multiplier *= stacks;
            c.flat *= stacks;
            return c;
        }
    }

    public BaseGameEffect effect;
    public OldValue[] effectValues = new OldValue[1];
    [SerializeReference] public ValueGroup valueGroup = new ValueGroup();
    public int effectDuration;
    public int activationLimit;
    public int stacks;
    public int maxStacks = -1;
    public string description;

    /// <summary>
    /// Shallow-Copy
    /// </summary>
    /// <returns></returns>
    public EffectProperties Copy()
    {
        return MemberwiseClone() as EffectProperties;
    }
}