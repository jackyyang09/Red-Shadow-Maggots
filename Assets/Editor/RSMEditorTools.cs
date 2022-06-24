using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class RSMEditorTools : Editor
{
    [MenuItem("RSM Tools/Open Save Folder")]
    public static void OpenSaveFolder()
    {
        Application.OpenURL(Application.persistentDataPath);
    }

    [MenuItem("RSM Tools/Wipe Save Data")]
    public static void WipeSaveData()
    {
        if (EditorUtility.DisplayDialog("Confirm Deletion", "Delete BattleData.xml and PlayerData.xml?", "Yes", "Cancel"))
        {
            if (File.Exists(PlayerDataManager.FILE_PATH)) File.Delete(PlayerDataManager.FILE_PATH);
            if (File.Exists(BattleStateManager.FILE_PATH)) File.Delete(BattleStateManager.FILE_PATH);
            EditorUtility.DisplayDialog("Done!", "Deletion Completion", "Neat!");
        }
    }
}