using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

public abstract class SerialReferenceDrawer<T> : PropertyDrawer where T : class
{
    protected readonly float DefaultHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.Enum, GUIContent.none) + 2;

    string[] typeNames;
    protected string[] TypeNames
    {
        get
        {
            if (typeNames == null)
            {
                typeNames = new string[ReferenceTypes.Count];
                for (int i = 0; i < ReferenceTypes.Count; i++)
                {
                    typeNames[i] = ReferenceTypes[i].Name;
                }
            }
            return typeNames;
        }
    }

    protected virtual List<Type> ReferenceTypes => new List<Type>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return DefaultHeight;

        var propCount = GetPropertyCount(property);

        return DefaultHeight * (propCount + 2);
    }

    int GetPropertyCount(SerializedProperty property)
    {
        int propCount = 0;
        SerializedProperty prop = property.Copy();
        SerializedProperty child = property.Copy().GetEndProperty();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            // Avoid drawing the top-level property again
            if (SerializedProperty.EqualContents(prop, child))
                break;

            enterChildren = false;
            propCount++;
        }

        return propCount;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int index = 0;
        if (property.managedReferenceValue != null)
        {
            var t = property.managedReferenceValue.GetType();
            index = ReferenceTypes.IndexOf(t);
        }
        else
        {
            property.managedReferenceValue = Activator.CreateInstance(ReferenceTypes[0]);
        }

        int propCount = Mathf.Max(1, GetPropertyCount(property));

        var h = EditorGUI.GetPropertyHeight(SerializedPropertyType.Generic, GUIContent.none);

        if (propCount > 1)
        {
            // Render Foldout
            var labelRect = new Rect(position);
            labelRect.height = h;

            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label);
        }

        if (property.isExpanded || propCount == 1)
        {
            // Render type popup button
            var lastRect = new Rect(position);
            lastRect.height = h;

            if (propCount > 1)
            {
                lastRect.y += h + 2f;
                EditorGUI.indentLevel++;
            }
            
            if (propCount == 1)
            {
                label = property.GUIContent();
            }
            else
            {
                label = new GUIContent("Type");
            }
            var cRect = EditorGUI.PrefixLabel(lastRect, label);
            cRect.xMin -= 15;

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(cRect, index, TypeNames);
            if (EditorGUI.EndChangeCheck())
            {
                var oldInstance = property.managedReferenceValue as T;
                object newManagedRef = Activator.CreateInstance(ReferenceTypes[index]);
                var newInstance = newManagedRef as T;

                if (newInstance != null)
                {
                    ReflectionCopy.CopyFields(oldInstance, newInstance);
                    property.managedReferenceValue = newInstance;
                }

                return;
            }

            // Iterate through each sub-property
            SerializedProperty child = property.Copy().GetEndProperty();
            bool enterChildren = true;

            // Render sub-properties
            while (property.NextVisible(enterChildren))
            {
                // Avoid drawing the top-level property again
                if (SerializedProperty.EqualContents(property, child))
                    break;

                enterChildren = false;

                lastRect = new Rect(lastRect);
                lastRect.y += h + 2f;
                cRect = EditorGUI.PrefixLabel(lastRect, property.GUIContent());
                cRect.xMin -= 15;
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    EditorGUI.PropertyField(lastRect, property, GUIContent.none);
                }
                else
                {
                    EditorGUI.PropertyField(cRect, property, GUIContent.none);
                }
            }

            if (propCount > 1)
            {
                EditorGUI.indentLevel--;
            }
        }
    }
}

public static class ReflectionCopy
{
    public static void CopyFields<T>(T source, T target)
    {
        FieldInfo[] sourceFields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo[] targetFields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var sField in sourceFields)
        {
            foreach (var tField in targetFields)
            {
                if (sField.Name == tField.Name && sField.FieldType == tField.FieldType)
                {
                    tField.SetValue(target, sField.GetValue(source));
                    break;
                }
            }
        }
    }
}