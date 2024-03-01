using UnityEngine;
using UnityEditor;

public class SaveScriptableObjects : EditorWindow
{
    [MenuItem("Tools/Save Scriptable Objects")]
    private static void SaveAllScriptableObjects()
    {
        // Find all ScriptableObject assets in the project
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (scriptableObject != null)
            {
                // Mark the ScriptableObject as dirty to prompt Unity to save it
                EditorUtility.SetDirty(scriptableObject);
            }
        }

        // Save the project to ensure changes are persisted
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All ScriptableObjects saved.");
    }
}