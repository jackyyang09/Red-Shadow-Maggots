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
        Percentage,
        Value,
        Decimal
    }

    [System.Serializable]
    public class EffectValue
    {
        public float multiplier;
        public float flat;
        public EffectType flatType;
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