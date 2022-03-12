using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BrainFailProductions.PolyFew;

public class MultiObjectPropertyEditor : EditorWindow
{
    string propertyName;
    List<SerializedObject> serializedObjects;

    static MultiObjectPropertyEditor window;

    [MenuItem("Tools/Multi-Object Property Editor")]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        window = GetWindow<MultiObjectPropertyEditor>();
        window.Show();
        window.Focus();
        window.titleContent.text = "Multi-Object Property Editor";
    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        LookupProperties();
        if (HasOpenInstances<MultiObjectPropertyEditor>())
        {
            GetWindow<MultiObjectPropertyEditor>().Repaint();
        }
    }

    void LookupProperties()
    {
        serializedObjects = new List<SerializedObject>();
        property = null;

        for (int i = 0; i < Selection.objects.Length; i++)
        {
            var effect = Selection.objects[i] as BaseGameEffect;
            if (effect)
            {
                serializedObjects.Add(new SerializedObject(effect));
            }
        }
    }

    SerializedProperty property;
    private void OnGUI()
    {
        propertyName = EditorGUILayout.TextField("Property Name", propertyName);
        if (GUILayout.Button("Lookup Property"))
        {
            if (Selection.objects.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Select objects first!", "OK");
                return;
            }
            
            if (serializedObjects.Count > 0)
            {
                var prop = serializedObjects[0].FindProperty(propertyName);
                if (prop != null)
                {
                    property = prop;
                }
            }
        }

        if (Selection.objects.Length == 0) return;

        if (serializedObjects == null)
        {
            LookupProperties();
        }
        if (property != null)
        {
            EditorGUILayout.PropertyField(property);
        }

        if (GUILayout.Button("Apply Property to Selections"))
        {
            for (int i = 0; i < serializedObjects.Count; i++)
            {
                serializedObjects[i].FindProperty(propertyName).SetObjectValue(property.GetValue());
                serializedObjects[i].ApplyModifiedProperties();
            }
            Close();
        }
    }
}