using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PositionListTool)), CanEditMultipleObjects]
public class PositionListToolEditor : Editor
{
    SerializedProperty positionArray;

    public static bool hideTransformTool;

    protected virtual void OnSceneGUI()
    {
        PositionListTool myScript = (PositionListTool)target;

        if (myScript.GetPositions().Count > 1) Handles.DrawPolyLine(myScript.GetPositions().ToArray());

        for (int i = 0; i < myScript.GetPositions().Count; i++)
        {
            Undo.RecordObject(myScript, "Changed position");
            myScript.SetPositionAtIndex(i, Handles.PositionHandle(myScript.GetPositionAtIndex(i), Quaternion.identity));
        }
    }

    private void OnEnable()
    {
        positionArray = serializedObject.FindProperty("positions");
        Tools.hidden = hideTransformTool;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PositionListTool myScript = (PositionListTool)target;

        bool hideTool = EditorGUILayout.Toggle("Hide Transform Tool", hideTransformTool);
        if (hideTool != hideTransformTool)
        {
            hideTransformTool = hideTool;
            Tools.hidden = hideTransformTool;
            if (SceneView.sceneViews.Count > 0) SceneView.lastActiveSceneView.Repaint();
        }

        DrawPropertiesExcluding(serializedObject, new string[] { "m_Script" });

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Position At Local Origin"))
        {
            int index = positionArray.arraySize;
            positionArray.InsertArrayElementAtIndex(index);
            SerializedProperty pos = positionArray.GetArrayElementAtIndex(index);
            pos.vector3Value = myScript.transform.position;
        }

        if (GUILayout.Button("Add Position at World Origin"))
        {
            int index = positionArray.arraySize;
            positionArray.InsertArrayElementAtIndex(index);
            SerializedProperty pos = positionArray.GetArrayElementAtIndex(index);

            pos.vector3Value = Vector3.zero;
        }
        EditorGUILayout.EndHorizontal();

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}