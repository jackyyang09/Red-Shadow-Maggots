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

    SerializedProperty abilityName;
    SerializedProperty targetMode;
    SerializedProperty coolDown;
    SerializedProperty gameEffects;

    public override void InitializeSerializedProperties()
    {
        abilityName = serializedObject.FindProperty(nameof(abilityName));
        sprite = serializedObject.FindProperty(nameof(sprite));
        targetMode = serializedObject.FindProperty(nameof(targetMode));
        coolDown = serializedObject.FindProperty(nameof(coolDown));
        gameEffects = serializedObject.FindProperty(nameof(gameEffects));
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

        var skillDescriptions = targetObject.GetSkillDescriptions();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        RenderEffectDescriptions((TargetMode)targetMode.enumValueIndex, targetObject.gameEffects, skillDescriptions);
        EditorGUILayout.EndVertical();

        RenderEffectProperties(targetObject.gameEffects, gameEffects);

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }
}