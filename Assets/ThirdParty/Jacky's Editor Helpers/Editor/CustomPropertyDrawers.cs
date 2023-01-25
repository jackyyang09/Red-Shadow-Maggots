using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

[CustomPropertyDrawer(typeof(DragAndDropString))]
public class DragAndDropStringDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        EditorGUI.BeginChangeCheck();
        var newName = EditorGUI.TextField(position, property.stringValue);
        if (EditorGUI.EndChangeCheck())
        {
            property.stringValue = newName;
        }
        else
        {
            if (EditorHelper.DragAndDropRegion(position, string.Empty, string.Empty))
            {
                if (DragAndDrop.objectReferences.Length > 0)
                {
                    var obj = DragAndDrop.objectReferences[0];
                    property.stringValue = obj.name;
                }
            }
        }

        EditorGUI.EndProperty();
    }
}