using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class EffectGroup
{
    public EffectProperties damageProps;
    public EffectProperties effectProps;
    [SerializeReference] public BaseApplicationStyle appStyle = new BaseApplicationStyle();
    public TargetMode targetOverride;
    [SerializeReference] public BaseEffectTarget effectTarget;

    public EffectGroup Copy()
    {
        var group = MemberwiseClone() as EffectGroup;
        group.damageProps = damageProps.Copy();
        group.effectProps = effectProps.Copy();
        return group;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EffectGroup))]
public class EffectGroupDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (property.managedReferenceValue == null)
        {
            property.managedReferenceValue = new EffectGroup();
        }

        EditorGUI.PropertyField(position, property, true);

        EditorGUI.EndProperty();
    }
}
#endif