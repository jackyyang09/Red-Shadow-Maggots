using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CanEditMultipleObjects]
[CustomEditor(typeof(ResolutionSpecificElementProperties))]
public class RSEEditor : Editor
{
    SerializedProperty dataPairs;
    SerializedObject element;

    private void OnEnable()
    {
        dataPairs = serializedObject.FindProperty(nameof(dataPairs));

        element = new SerializedObject(serializedObject.FindProperty("element").objectReferenceValue);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < dataPairs.arraySize; i++)
        {
            SerializedProperty pair = dataPairs.GetArrayElementAtIndex(i);
            var aspectRatio = pair.FindPropertyRelative("aspectRatio").vector2Value;
            float normalizedAR = aspectRatio.x / aspectRatio.y;
            if (GUILayout.Button("Apply " + aspectRatio.ToString()))
            {
                for (int j = 0; j < Selection.gameObjects.Length; j++)
                {
                    ResolutionSpecificElementProperties p;
                    if (Selection.gameObjects[j].TryGetComponent(out p))
                    {
                        p.ApplyDataPair(normalizedAR);
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        base.OnInspectorGUI();

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }
}
#endif

[RequireComponent(typeof(CustomLayoutElement))]
public class ResolutionSpecificElementProperties : MonoBehaviour
{
    [System.Serializable]
    public struct ResolutionDataPair
    {
        public Vector2 aspectRatio;
        public float normalizedAspectRatio { get { return aspectRatio.x / aspectRatio.y; } }
        public float widthOverride;
        public float heightOverride;
        public Vector2 positionalOffset;
    }

    [SerializeField] ResolutionDataPair[] dataPairs;

    [SerializeField] CustomLayoutElement element;

#if UNITY_EDITOR
    [MenuItem("Tools/Print Resolution")]
    public static void ShowResolution()
    {
        UnityEditor.EditorUtility.DisplayDialog("Resolution", new Vector2(Screen.width, Screen.height).ToString(), "OK");
    }

    private void OnValidate()
    {
        if (element == null) element = GetComponent<CustomLayoutElement>();
    }
#endif

    private void Awake()
    {
        float aspectRatio = (float)Screen.width / (float)Screen.height;

        for (int i = 0; i < dataPairs.Length; i++)
        {
            if (Mathf.Approximately(dataPairs[i].normalizedAspectRatio, aspectRatio))
            {
                element.OverrideData(dataPairs[i]);
                break;
            }
        }
    }

#if UNITY_EDITOR
    public void ApplyDataPair(float aspectRatio)
    {
        for (int i = 0; i < dataPairs.Length; i++)
        {
            if (Mathf.Approximately(dataPairs[i].normalizedAspectRatio, aspectRatio))
            {
                Undo.RecordObject(element, "Applied Data Pair");
                element.OverrideData(dataPairs[i]);
                break;
            }
        }
    }
#endif
}