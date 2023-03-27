using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadObjectName))]
public class ReadObjectNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            base.OnGUI(position, property, label);
        }
        else
        {
            Rect halfRect = new Rect(position);
            EditorGUI.PrefixLabel(halfRect, label);
            halfRect.width *= 0.66f;
            EditorGUI.PropertyField(halfRect, property);
            halfRect.position += new Vector2(halfRect.width + 0.01f * position.width, 0);
            halfRect.width = position.width * 0.33f;
            if (GUI.Button(halfRect, "Use Object Name"))
            {
                property.stringValue = property.serializedObject.targetObject.name;
            }
        }
    }
}
#endif

public class ReadObjectName : PropertyAttribute
{
}