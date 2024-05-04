using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ValueGroup))]
public class ValueGroupDrawer : PropertyDrawer
{
    protected readonly float DefaultHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.Enum, GUIContent.none) + 2;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (property.managedReferenceValue == null)
        {
            property.managedReferenceValue = new ValueGroup();
        }

        EditorGUI.PropertyField(position, property, true);

        EditorGUI.EndProperty();
    }
}