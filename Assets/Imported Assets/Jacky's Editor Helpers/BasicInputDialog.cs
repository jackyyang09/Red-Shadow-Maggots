#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class BasicInputDialog : EditorWindow
{
    string text = "";
    GUIContent label;
    GUIContent buttonContent = new GUIContent("Submit");

    public static System.Action<string> OnSubmit;
    public static System.Action Test;

    public static void Initialize(string text, string defaultText = "", GUIContent buttonContent = null)
    {
        var window = CreateInstance<BasicInputDialog>();
        window.titleContent.text = text;
        window.text = defaultText;
        if (buttonContent != null) window.buttonContent = buttonContent;
        window.maxSize = new Vector2(500, 100);
        window.ShowModalUtility();
    }

    private void OnGUI()
    {
        text = EditorGUILayout.TextField(text);
        if (GUILayout.Button(buttonContent))
        {
            OnSubmit?.Invoke(text);
            GetWindow<BasicInputDialog>().Close();
        }
    }
}

#endif