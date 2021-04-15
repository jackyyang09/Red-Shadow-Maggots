using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(OptimizedCanvas))]
public class OptimizedCanvasEditor : Editor
{
    OptimizedCanvas myScript;

    private void OnEnable()
    {
        myScript = (OptimizedCanvas)target;
    }

    public override void OnInspectorGUI()
    {
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

        base.OnInspectorGUI();
    }
}
