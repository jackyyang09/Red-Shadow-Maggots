using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using JackysEditorHelpers;

[CustomEditor(typeof(OptimizedTransitionTween))]
public class OptimizedTransitionTweenEditor : OptimizedTransitionBaseEditor
{
    class TweenTarget : System.IDisposable
    {
        public RectTransform rectTransform { get { return rect.objectReferenceValue as RectTransform; } }
        public SerializedProperty rect;
        public SerializedProperty outPosition;
        public SerializedProperty inPosition;

        public void Dispose()
        {
            outPosition = null;
            inPosition = null;
            rect = null;
        }
    }

    SerializedProperty tweeners;
    const string FOLDOUT_KEY = "OptimizedCanvas_TweenerFoldout";
    bool tweenersFoldout;

    OptimizedTransitionTween myScript;
    RectTransform canvas;

    GUIContent editIcon;
    GUIContent resetIcon;

    List<string> ignoredProperties = new List<string> { "m_Script" };

    TweenTarget currentTweenTarget;

    bool currentlyEditting { get { return target && currentTweenTarget != null && tweenersFoldout; } }

    List<RectTransform> cachedRects;

    private void OnEnable()
    {
        if (EditorPrefs.HasKey(FOLDOUT_KEY))
        {
            tweenersFoldout = EditorPrefs.GetBool(FOLDOUT_KEY);
        }

        myScript = target as OptimizedTransitionTween;
        canvas = myScript.transform.root.GetComponentInChildren<Canvas>().transform as RectTransform;

        tweeners = serializedObject.FindProperty(nameof(tweeners));
        ignoredProperties.Add(nameof(tweeners));

        editIcon = EditorGUIUtility.IconContent("d_editicon.sml");
        editIcon.tooltip = "Enable editing of in/out positions with scene Gizmos. Gizmos MUST be enabled to use this tool";
        resetIcon = EditorGUIUtility.IconContent("d_Refresh");
        resetIcon.tooltip = "Use anchoredPosition of rect";

        RebuildRectList();
    }

    void RebuildRectList()
    {
        cachedRects = new List<RectTransform>();
        for (int i = 0; i < tweeners.arraySize; i++)
        {
            SerializedProperty rect = tweeners.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(rect));
            if (rect.objectReferenceValue)
            {
                cachedRects.Add(rect.objectReferenceValue as RectTransform);
            }
        }
    }

    protected void OnSceneGUI()
    {
        if (!currentlyEditting) return;

        Vector3 point = Vector3.zero;
        Vector2 backupPosition = currentTweenTarget.rectTransform.anchoredPosition;

        currentTweenTarget.rectTransform.anchoredPosition = currentTweenTarget.inPosition.vector2Value;
        point = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas, currentTweenTarget.rectTransform).center;

        Vector3 inHandlePos = point * canvas.localScale.x;
        {
            inHandlePos.z = canvas.position.z;

            Vector2 delta = Handles.PositionHandle(inHandlePos, Quaternion.identity);

            delta -= (Vector2)(point * canvas.localScale.x);
            delta /= canvas.localScale.x;

            if (delta.magnitude > 0)
            {
                currentTweenTarget.inPosition.vector2Value += delta;
            }
        }

        currentTweenTarget.rectTransform.anchoredPosition = currentTweenTarget.outPosition.vector2Value;
        point = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas, currentTweenTarget.rectTransform).center;

        Vector3 outHandlePos = point * canvas.localScale.y;
        {
            outHandlePos.z = canvas.position.z;

            Vector2 delta = Handles.PositionHandle(outHandlePos, Quaternion.identity);

            delta -= (Vector2)(point * canvas.localScale.x);
            delta /= canvas.localScale.x;

            if (delta.magnitude > 0)
            {
                currentTweenTarget.outPosition.vector2Value += delta;
            }
        }

        Handles.DrawLine(inHandlePos, outHandlePos);

        currentTweenTarget.rectTransform.anchoredPosition = backupPosition;

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        EditorGUI.BeginChangeCheck();
        tweenersFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(tweenersFoldout, new GUIContent("Tweeners"));
        if (EditorGUI.EndChangeCheck()) EditorPrefs.SetBool(FOLDOUT_KEY, tweenersFoldout);
        if (tweenersFoldout)
        {
            int indexToDelete = -1;
            for (int i = 0; i < tweeners.arraySize; i++)
            {
                SerializedProperty rect = tweeners.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(rect));
                SerializedProperty outPosition = tweeners.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(outPosition));
                SerializedProperty inPosition = tweeners.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(inPosition));

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(rect.objectReferenceValue ? rect.objectReferenceValue.name : "Null");

                bool grayedOut = false;

                if (currentlyEditting)
                {
                    if (currentTweenTarget.rect.objectReferenceValue == rect.objectReferenceValue)
                    {
                        grayedOut = true;
                    }
                }

                Color backupColor = GUI.backgroundColor;
                if (grayedOut)
                {
                    GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                    if (GUILayout.Button(editIcon, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                    {
                        currentTweenTarget.Dispose();
                        currentTweenTarget = null;
                        UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>().Repaint();
                    }
                }
                else
                {
                    if (GUILayout.Button(editIcon, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                    {
                        if (currentTweenTarget != null) currentTweenTarget.Dispose();
                        currentTweenTarget = new TweenTarget();
                        currentTweenTarget.rect = rect;
                        currentTweenTarget.outPosition = outPosition;
                        currentTweenTarget.inPosition = inPosition;
                        gizmosEnabled = true;
                    }
                }
                GUI.backgroundColor = backupColor;

                if (GUILayout.Button(resetIcon, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                {
                    outPosition.vector2Value = (rect.objectReferenceValue as RectTransform).anchoredPosition;
                    inPosition.vector2Value = (rect.objectReferenceValue as RectTransform).anchoredPosition;
                }

                Color buttonColor = GUI.color;
                GUI.color = Color.red;
                if (GUILayout.Button(new GUIContent("X"), new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                {
                    indexToDelete = i;
                }
                GUI.color = buttonColor;

                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(rect);
                if (EditorGUI.EndChangeCheck())
                {
                    if (cachedRects.Contains(rect.objectReferenceValue as RectTransform))
                    {
                        EditorUtility.DisplayDialog(
                            "Optimized Transition Tweener Warning",
                            "This RectTransform is already listed as a Tweener!",
                            "OK");
                        rect.objectReferenceValue = null;
                    }
                    else
                    {
                        RebuildRectList();
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(outPosition);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(inPosition);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Tweener", new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                tweeners.AddAndReturnNewArrayElement().FindPropertyRelative("rect").objectReferenceValue = null;
            }

            if (indexToDelete != -1)
            {
                RectTransform rect = tweeners.GetArrayElementAtIndex(indexToDelete).FindPropertyRelative(nameof(rect)).objectReferenceValue as RectTransform;
                if (cachedRects.Contains(rect))
                {
                    cachedRects.Remove(rect);
                }
                tweeners.DeleteArrayElementAtIndex(indexToDelete);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        DrawPropertiesExcluding(serializedObject, ignoredProperties.ToArray());

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }

    static bool gizmosEnabled
    {
        get
        {
            return UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>().drawGizmos;
        }
        set
        {
            UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>().drawGizmos = value;
        }
    }
}
