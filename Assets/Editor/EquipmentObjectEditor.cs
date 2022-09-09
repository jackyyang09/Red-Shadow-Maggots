using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

[CustomEditor(typeof(EquipmentObject))]
public class EquipmentObjectEditor : BaseEffectEditor<EquipmentObject>
{
    protected override string SHOW_GAMEEFFECTS { get { return "EQUIPMENTOBJECT_GAMEEFFECTS"; } }

    SerializedProperty equipName;
    SerializedProperty timing;
    SerializedProperty battlePhase;
    SerializedProperty gameEffects;

    public override void InitializeSerializedProperties()
    {
        equipName = serializedObject.FindProperty(nameof(equipName));
        sprite = serializedObject.FindProperty(nameof(sprite));
        timing = serializedObject.FindProperty(nameof(timing));
        battlePhase = serializedObject.FindProperty(nameof(battlePhase));
        gameEffects = serializedObject.FindProperty(nameof(gameEffects));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        RenderDragAndDropSprite();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(equipName);
        if (EditorHelper.CondensedButton("Use Object Name"))
        {
            equipName.stringValue = target.name;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(timing);
        
        if (timing.enumValueIndex == (int)EffectActivationTiming.BattlePhase)
        {
            EditorGUILayout.PropertyField(battlePhase);
        }

        RenderEffectProperties(targetObject.gameEffects, gameEffects);

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }
}