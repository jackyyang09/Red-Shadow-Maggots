using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BaseEffectValue))]
public class EffectValueDrawer : SerialReferenceDrawer<BaseEffectValue>
{
    protected override List<Type> ReferenceTypes => new List<Type> 
    { 
        typeof(BaseEffectValue),
        typeof(StatScaledValue),
        typeof(StackCountValue)
    };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        base.OnGUI(position, property, label);

        EditorGUI.EndProperty();
    }
}