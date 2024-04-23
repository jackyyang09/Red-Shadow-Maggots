using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BaseEffectTarget))]
public class EffectTargetEditor : SerialReferenceDrawer<BaseEffectTarget>
{
    protected override List<Type> ReferenceTypes => new List<Type>
    {
        typeof(TargetSelf),
        typeof(TargetTarget),
        typeof(AllAllies),
        typeof(AllAlliesExceptCaster),
        typeof(AllAlliesExceptTarget),
        typeof(AllEnemies)
    };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        base.OnGUI(position, property, label);

        EditorGUI.EndProperty();
    }
}