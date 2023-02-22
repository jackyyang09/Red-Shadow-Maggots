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

    SerializedProperty onCanvasShow;
    SerializedProperty onCanvasStartHide;
    SerializedProperty onCanvasHide;

    List<string> excludedProperties = new List<string>(new string[] { "m_Script" });

    static string SHOWKEY = nameof(OptimizedCanvas) + nameof(SHOWKEY);
    bool ShowUnityEvents
    {
        get => EditorPrefs.GetBool(SHOWKEY, true);
        set => EditorPrefs.SetBool(SHOWKEY, value);
    }

    private void OnEnable()
    {
        myScript = (OptimizedCanvas)target;

        caster = serializedObject.FindProperty(nameof(caster));
        excludedProperties.Add(nameof(caster));
        onCanvasShow = serializedObject.FindProperty(nameof(onCanvasShow));
        excludedProperties.Add(nameof(onCanvasShow));
        onCanvasStartHide = serializedObject.FindProperty(nameof(onCanvasStartHide));
        excludedProperties.Add(nameof(onCanvasStartHide));
        onCanvasHide = serializedObject.FindProperty(nameof(onCanvasHide));
        excludedProperties.Add(nameof(onCanvasHide));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUIStyle style = new GUIStyle(EditorStyles.label);
        style.alignment = TextAnchor.MiddleCenter;
        if (myScript.IsVisible)
        {
            style.normal.textColor = Color.green;
            EditorGUILayout.LabelField("Canvas State: Visible", style);
        }
        else
        {
            style.normal.textColor = Color.grey;
            EditorGUILayout.LabelField("Canvas State: Not Visible", style);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Show Canvas"))
        {
            myScript.EditorShow();

            var selected = Selection.gameObjects;
            for (int i = 0; i < selected.Length; i++)
            {
                OptimizedCanvas selectedCanvas = null;
                if (selected[i].TryGetComponent(out selectedCanvas))
                {
                    selectedCanvas.RegisterUndo();
                    selectedCanvas.EditorShow();
                }
            }
        }
        else if (GUILayout.Button("Hide Canvas"))
        {
            List<GameObject> selected = new List<GameObject>(Selection.gameObjects);
            for (int i = 0; i < selected.Count; i++)
            {
                OptimizedCanvas selectedCanvas = null;
                if (selected[i].TryGetComponent(out selectedCanvas))
                {
                    selectedCanvas.RegisterUndo();
                    selectedCanvas.EditorHide();
                }
            }
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

        DrawPropertiesExcluding(serializedObject, excludedProperties.ToArray());

        ShowUnityEvents = EditorGUILayout.BeginFoldoutHeaderGroup(ShowUnityEvents, "Show Unity Events");
        if (ShowUnityEvents)
        {
            EditorGUILayout.PropertyField(onCanvasShow);
            EditorGUILayout.PropertyField(onCanvasStartHide);
            EditorGUILayout.PropertyField(onCanvasHide);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}