using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(OptimizedCanvas))]
public class OptimizedCanvasEditor : Editor
{
    OptimizedCanvas myScript;

    SerializedProperty caster;

    private void OnEnable()
    {
        myScript = (OptimizedCanvas)target;

        caster = serializedObject.FindProperty("caster");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Show Canvas"))
        {
            myScript.EditorButtonShow();
        }
        else if (GUILayout.Button("Hide Canvas"))
        {
            myScript.EditorHide();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(caster);
        if (caster.objectReferenceValue == null)
        {
            if (GUILayout.Button("Add New", new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                caster.objectReferenceValue = myScript.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }
        else
        {
            if (GUILayout.Button("Get Reference", new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                caster.objectReferenceValue = myScript.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }

        EditorGUILayout.EndHorizontal();

        DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", "caster" });

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
