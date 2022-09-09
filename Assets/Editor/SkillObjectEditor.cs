using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(SkillObject))]
public class SkillObjectEditor : BaseEffectEditor<SkillObject>
{
    protected override string SHOW_GAMEEFFECTS { get { return "SKILLOBJECT_GAMEEFFECTS"; } }

    SerializedProperty skillName;
    SerializedProperty skillDescription;
    SerializedProperty targetMode;
    SerializedProperty skillCooldown;
    SerializedProperty gameEffects;

    public override void InitializeSerializedProperties()
    {
        skillName = serializedObject.FindProperty(nameof(skillName));
        skillDescription = serializedObject.FindProperty(nameof(skillDescription));
        sprite = serializedObject.FindProperty(nameof(sprite));
        targetMode = serializedObject.FindProperty(nameof(targetMode));
        skillCooldown = serializedObject.FindProperty(nameof(skillCooldown));
        gameEffects = serializedObject.FindProperty(nameof(gameEffects));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        RenderDragAndDropSprite();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(skillName);
        if (EditorHelper.CondensedButton("Use Object Name"))
        {
            skillName.stringValue = target.name;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(targetMode);
        EditorGUILayout.PropertyField(skillCooldown);

        var skillDescriptions = targetObject.GetSkillDescriptions();

        RenderEffectDescriptions(targetObject.gameEffects, skillDescriptions);
        RenderEffectProperties(targetObject.gameEffects, gameEffects);

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }
}