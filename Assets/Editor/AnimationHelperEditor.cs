using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationHelper))]
public class AnimationHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Ragdoll Wizard"))
        {
            RagdollWizard.CreateWizard(target as AnimationHelper);
        }

        DrawDefaultInspector();
    }
}
