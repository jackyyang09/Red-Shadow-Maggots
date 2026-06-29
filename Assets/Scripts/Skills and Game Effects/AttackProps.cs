using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackProps
{
    [SerializeReference, SubclassSelector] public BaseEffectValue TotalDamage;
    public float[] Hits;

    public float GetDamage(int hitID, TargetProps props) => TotalDamage.GetValue(props) * Hits[hitID];

    public AttackProps Copy()
    {
        var clone = (AttackProps)MemberwiseClone();

        if (TotalDamage != null) clone.TotalDamage = TotalDamage.Clone();

        return clone;
    }
}