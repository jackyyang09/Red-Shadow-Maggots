using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OptimizedTransitionFade))]
public class OptimizedTransitionFadeEditor : OptimizedTransitionBaseEditor
{
    OptimizedTransitionFade myScript;

    SerializedProperty canvasGroup;

    private void OnEnable()
    {
        myScript = target as OptimizedTransitionFade;

        canvasGroup = serializedObject.FindProperty(nameof(canvasGroup));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();

        GUIContent content = new GUIContent(GUIContent.none);

        EditorGUILayout.PropertyField(canvasGroup);
        if (canvasGroup.objectReferenceValue == null)
        {
            content.text = "Add New";
        }
        else
        {
            content.text = "Find Reference";
        }

        if (GUILayout.Button(content, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
        {
            canvasGroup.objectReferenceValue = myScript.GetComponent<CanvasGroup>();
            CanvasGroup newCV = null;
            if (myScript.TryGetComponent(out newCV))
            {
                canvasGroup.objectReferenceValue = newCV;
            }
            else
            {
                canvasGroup.objectReferenceValue = myScript.gameObject.AddComponent<CanvasGroup>();
            }
        }

        EditorGUILayout.EndHorizontal();

        DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", nameof(canvasGroup) });

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }
}
