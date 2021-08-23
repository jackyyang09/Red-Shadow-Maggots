using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;

[CustomEditor(typeof(TextureEyeController))]
public class TextureEyeEditor : Editor
{
    SerializedProperty drawEyeGizmos;

    SerializedProperty FindProp(string prop) => serializedObject.FindProperty(prop);

    List<string> ignoredProperties = new List<string>() { "m_Script" };

    TextureEyeController myScript;

    public static bool updating = false;

    static TextureEyeEditor()
    {
        updating = false;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChangedEditMode;
    }

    public static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        updating = false;
    }

    public static void OnActiveSceneChangedEditMode(Scene arg0, Scene arg1)
    {
        EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChangedEditMode;
        updating = false;
    }

    private void OnEnable()
    {
        myScript = target as TextureEyeController;

        drawEyeGizmos = FindProp(nameof(drawEyeGizmos));
        //ignoredProperties.Add(nameof(drawEyeGizmos));

        myScript.CreateMaterialInstancesInEditor();
        if (!updating)
        {
            EditorApplication.update += Update;
            updating = true;
        }
    }

    private void OnDisable()
    {
        
    }

    void Update()
    {
        if (!updating) EditorApplication.update -= Update;
        myScript.UpdateEyeLookDirection();
    }

    private void OnSceneGUI()
    {
        if (!target) return;
        if (!myScript.enabled) return;

        if (drawEyeGizmos.boolValue)
        {
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = myScript.ToHeadSpace(Handles.PositionHandle(myScript.LeftEyeWorldPosition, Quaternion.identity));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(myScript, "Moved left eye handle");
                    myScript.leftEyePos = newPos;
                }
            }

            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = myScript.ToHeadSpace(Handles.PositionHandle(myScript.RightEyeWorldPosition, Quaternion.identity));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(myScript, "Moved right eye handle");
                    myScript.rightEyePos = newPos;
                }
            }
        }

        switch (myScript.eyeLookMode)
        {
            case TextureEyeController.EyeLookMode.Both:
                {
                    if (myScript.lookTarget)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPos = Handles.PositionHandle(myScript.lookTarget.position, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(myScript.lookTarget, "Moved eye target");
                            myScript.lookTarget.position = newPos;
                        }
                    }
                }
                break;
            case TextureEyeController.EyeLookMode.Separate:
                {
                    if (myScript.leftTarget)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPos = Handles.PositionHandle(myScript.leftTarget.position, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(myScript.leftTarget, "Moved eye target");
                            myScript.leftTarget.position = newPos;
                        }
                    }
                    if (myScript.rightTarget)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPos = Handles.PositionHandle(myScript.rightTarget.position, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(myScript.rightTarget, "Moved eye target");
                            myScript.rightTarget.position = newPos;
                        }
                    }
                }
                break;
        }
        
    }

    public override void OnInspectorGUI()
    {
        if (myScript == null) return;

        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, ignoredProperties.ToArray());

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
