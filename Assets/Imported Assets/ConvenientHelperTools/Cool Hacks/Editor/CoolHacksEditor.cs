using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(CoolHacks))]
public class CoolHacksEditor : Editor
{
    SerializedProperty hacks;
    SerializedProperty hackLogger;

    CoolHacks myScript;

    private void OnEnable()
    {
        myScript = (CoolHacks)target;

        hacks = serializedObject.FindProperty("hacks");
        hackLogger = serializedObject.FindProperty("hackLogger");
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Setup"))
        {
            Setup();
        }

        GUILayout.Label(new GUIContent("Hacks Enabled: " + CoolHacks.hacksEnabled.ToString()));

        if (Application.isPlaying)
        {
            for (int i = 0; i < hacks.arraySize; i++)
            {
                if (GUILayout.Button(hacks.GetArrayElementAtIndex(i).FindPropertyRelative("hackText").stringValue))
                {
                    myScript.InvokeHack(i);
                }
            }
            DrawPropertiesExcluding(serializedObject, new string[] { "m_Script" });
        }
        else
        {
            DrawPropertiesExcluding(serializedObject, new string[] { "m_Script" });
        }

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    void Setup()
    {
        if (!hackLogger.objectReferenceValue)
        {
            if (myScript.gameObject.name == "GameObject") myScript.gameObject.name = "CoolHacks";
            hackLogger.objectReferenceValue = myScript.GetComponentInChildren<Text>();
            if (!hackLogger.objectReferenceValue)
            {
                GameObject g = new GameObject("HackLogger");

                var text = g.AddComponent<Text>();
                g.transform.parent = myScript.transform;
                text.resizeTextForBestFit = true;
                text.raycastTarget = false;
                text.fontSize = 100;
                text.resizeTextMaxSize = 150;
                text.text = "Hacker Voice: I'm in...";
                hackLogger.objectReferenceValue = text;

                var textRect = (RectTransform)g.transform;
                textRect.anchorMin = new Vector2(0, 1);
                textRect.anchorMax = new Vector2(0, 1);
                textRect.pivot = new Vector3(0, 1);
                textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1920);
                textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 150);
                textRect.anchoredPosition = Vector2.zero;
            }
        }
    }
}
