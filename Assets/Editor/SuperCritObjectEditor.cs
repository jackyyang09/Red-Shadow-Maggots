using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

[CustomEditor(typeof(SuperCritObject))]
public class SuperCritObjectEditor : BaseEffectEditor<SuperCritObject>
{
    protected override string SHOW_GAMEEFFECTS => "SUPERCRITOBJECT_GAMEEFFECTS";

    SerializedProperty abilityName;
    SerializedProperty skillDescription;
    SerializedProperty targetMode;
    SerializedProperty gameEffects;
    SerializedProperty damageEffects;

    public override void InitializeSerializedProperties()
    {
        abilityName = serializedObject.FindProperty(nameof(abilityName));
        skillDescription = serializedObject.FindProperty(nameof(skillDescription));
        sprite = serializedObject.FindProperty(nameof(sprite));
        targetMode = serializedObject.FindProperty(nameof(targetMode));
        gameEffects = serializedObject.FindProperty(nameof(gameEffects));
        damageEffects = serializedObject.FindProperty(nameof(damageEffects));
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

        var damageDesc = targetObject.GetEffectDescription();
        var effectDesc = targetObject.GetSkillDescriptions();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(damageDesc, BuffStyle);
        RenderEffectDescriptions(targetObject.targetMode, targetObject.gameEffects, effectDesc);
        EditorGUILayout.EndVertical();

        RenderEffectProperties(targetObject.damageEffects, damageEffects);
        RenderEffectProperties(targetObject.gameEffects, gameEffects);

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }
}