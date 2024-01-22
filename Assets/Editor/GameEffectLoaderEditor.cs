using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

[CustomEditor(typeof(GameEffectLoader))]
public class GameEffectLoaderEditor : Editor
{
    SerializedProperty gameEffects;

    private void OnEnable()
    {
        gameEffects = serializedObject.FindProperty(nameof(gameEffects));
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (GUILayout.Button("Load All Effects"))
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(BaseGameEffect));
            gameEffects.ClearArray();
            foreach (var g in guids)
            {
                var a = AssetDatabase.LoadAssetAtPath<BaseGameEffect>(AssetDatabase.GUIDToAssetPath(g));
                gameEffects.AddAndReturnNewArrayElement().objectReferenceValue = a;
            }
        }

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
