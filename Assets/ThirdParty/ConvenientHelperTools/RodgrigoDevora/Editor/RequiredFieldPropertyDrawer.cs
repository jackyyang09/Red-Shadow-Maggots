using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RequiredField))]
public class RequiredFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        RequiredField field = (RequiredField)attribute;

        if (property.objectReferenceValue == null)
        {
            GUI.color = field.color;
            EditorGUI.PropertyField(position, property, label);
            GUI.color = Color.white;
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
