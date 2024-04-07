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

[System.Serializable]
public class EffectProperties
{
    public enum EffectType
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
    public class EffectValue
    {
        [Tooltip("This value will be multiplied with a different value")]
        public float multiplier;
        [Tooltip("Any values to be added on")]
        public float flat;
        [Tooltip("Defines how the flat value is displayed in Skill Descriptions")]
        public EffectType flatType;
        [Tooltip("Defines how the value change is displayed in the Effect Description" +
            "\nex. ATK increased by 20 vs ATK increased by 20%")]
        public EffectType deltaType;

        public EffectValue Copy()
        {
            return MemberwiseClone() as EffectValue;
        }

        public static EffectValue operator *(EffectValue e, int stacks)
        {
            var c = e.Copy();
            c.multiplier *= stacks;
            c.flat *= stacks;
            return c;
        }

        public static EffectValue operator *(EffectValue e, float stacks)
        {
            var c = e.Copy();
            c.multiplier *= stacks;
            c.flat *= stacks;
            return c;
        }
    }

    public BaseGameEffect effect;
    public float[] values = new float[1];
    public EffectValue[] effectValues = new EffectValue[1];
    public int effectDuration;
    public int activationLimit;
    public int stacks;
    public int maxStacks = -1;
    public TargetMode targetOverride;

    /// <summary>
    /// Shallow-Copy
    /// </summary>
    /// <returns></returns>
    public EffectProperties Copy()
    {
        return MemberwiseClone() as EffectProperties;
    }
}