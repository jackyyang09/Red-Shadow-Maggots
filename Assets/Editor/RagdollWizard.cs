#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RagdollWizard : ScriptableWizard
{
    [SerializeField] GameObject[] ragdoll;
    [SerializeField] GameObject[] nonRagdoll;

    static SerializedObject Helper;

    public static void CreateWizard(AnimationHelper target)
    {
        Helper = new SerializedObject(target);
        DisplayWizard<RagdollWizard>("Ragdoll Wizard", "Apply");
    }

    void OnWizardCreate()
    {
        Helper.Update();

        if (ragdoll.Length > 0)
        {
            var r = Helper.FindProperty("ragdollRenderers");
            var renderers = new List<Renderer>();
            for (int i = 0; i < ragdoll.Length; i++)
            {
                var elements = ragdoll[i].GetComponentsInChildren<Renderer>();
                if (elements != null) renderers.AddRange(elements);
            }
            r.arraySize = renderers.Count;
            for (int i = 0; i < renderers.Count; i++)
            {
                r.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }
        }
        
        if (nonRagdoll.Length > 0)
        {
            var nr = Helper.FindProperty("nonRagdollRenderers");
            var renderers = new List<Renderer>();
            for (int i = 0; i < nonRagdoll.Length; i++)
            {
                var elements = nonRagdoll[i].GetComponentsInChildren<Renderer>();
                if (elements != null) renderers.AddRange(elements);
            }
            nr.arraySize = renderers.Count;
            for (int i = 0; i < renderers.Count; i++)
            {
                nr.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }
        }

        {
            var rb = Helper.FindProperty("rigidBodies");
            var rigidBodies = ((AnimationHelper)Helper.targetObject).GetComponentsInChildren<Rigidbody>();
            rb.arraySize = rigidBodies.Length;
            for (int i = 0; i < rigidBodies.Length; i++)
            {
                rb.GetArrayElementAtIndex(i).objectReferenceValue = rigidBodies[i];
            }
        }

        {
            var c = Helper.FindProperty("colliders");
            var colliders = ((AnimationHelper)Helper.targetObject).GetComponentsInChildren<Collider>();
            c.arraySize = colliders.Length;
            for (int i = 0; i < colliders.Length; i++)
            {
                c.GetArrayElementAtIndex(i).objectReferenceValue = colliders[i];
            }
        }

        Helper.ApplyModifiedProperties();
    }
}
#endif