using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(SkillObject))]
public class SkillObjectEditor : BaseEffectEditor<SkillObject>
{
    protected override string SHOW_GAMEEFFECTS => "SKILLOBJECT_GAMEEFFECTS";

    SerializedProperty targetMode;
    SerializedProperty coolDown;

    public override void InitializeSerializedProperties()
    {
        base.InitializeSerializedProperties();

        targetMode = serializedObject.FindProperty(nameof(targetMode));
        coolDown = serializedObject.FindProperty(nameof(coolDown));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        RenderDragAndDropSprite();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(abilityName);
        if (EditorHelper.CondensedButton("Use Object Name"))
        {
            abilityName.stringValue = target.name;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(targetMode);
        EditorGUILayout.PropertyField(coolDown);
        RenderConditionProperty();

        var skillDescriptions = targetObject.GetSkillDescriptions();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        RenderEffectDescriptions((TargetMode)targetMode.enumValueIndex, targetObject.effects, skillDescriptions);
        EditorGUILayout.EndVertical();

        EditorGUILayout.PropertyField(effects);
        EditorGUILayout.PropertyField(events);

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }
}