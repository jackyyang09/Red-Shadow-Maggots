using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ValueGroup
{
    [SerializeReference] public BaseEffectValue[] Values = new BaseEffectValue[] { new BaseEffectValue() };

    public float GetValue(TargetProps targetProps)
    {
        float o = 0;
        foreach (var v in Values)
        {
            o += v.GetValue(targetProps);
        }
        return o;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ValueGroup))]
public class ValueGroupDrawer : PropertyDrawer
{
    protected readonly float DefaultHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.Enum, GUIContent.none) + 2;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true) + DefaultHeight;
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