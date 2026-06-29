using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class EffectGroup
{
    [SerializeReference, SubclassSelector] public EffectProperties effectProps;
    [SerializeReference, SubclassSelector] public AttackProps attackProps;
    [SerializeReference, SubclassSelector] public BaseApplicationStyle appStyle;
    [SerializeReference, SubclassSelector] public BaseEffectTarget effectTarget;

    public EffectGroup Copy()
    {
        var group = MemberwiseClone() as EffectGroup;
        if (group.effectProps != null) group.effectProps = effectProps.Copy();
        if (group.attackProps != null) group.attackProps = attackProps.Copy();
        return group;
    }
}