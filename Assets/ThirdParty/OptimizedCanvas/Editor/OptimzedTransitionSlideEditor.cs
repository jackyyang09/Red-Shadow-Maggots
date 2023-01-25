using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OptimizedTransitionSlide))]
public class OptimzedTransitionSlideEditor : OptimizedTransitionBaseEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        RenderTestButtons(target as OptimizedTransitionBase);

        DrawPropertiesExcluding(serializedObject, "m_Script");

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
