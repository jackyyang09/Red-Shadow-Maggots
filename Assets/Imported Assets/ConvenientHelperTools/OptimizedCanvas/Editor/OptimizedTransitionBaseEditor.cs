using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class OptimizedTransitionBaseEditor : Editor
{
    protected void RenderTestButtons(OptimizedTransitionBase transition)
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Test Transition In"))
        {
            transition.EditorTransitionIn();
        }
        if (GUILayout.Button("Test Transition Out"))
        {
            transition.EditorTransitionOut();
        }
        EditorGUILayout.EndHorizontal();
    }
}
