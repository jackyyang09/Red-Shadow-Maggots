﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class RSMEditorTools : Editor
{
    public const string RSM_TOOLS_MENU = "RSM Tools/";
    public const string SAVE_MENU = RSM_TOOLS_MENU + "Save Tools/";

    [MenuItem(SAVE_MENU + "Open Save Folder")]
    public static void OpenSaveFolder()
    {
        Application.OpenURL(Application.persistentDataPath);
    }

    [MenuItem(SAVE_MENU + "Battle State")]
    public static void OpenBattleState()
    {
        Application.OpenURL(Application.persistentDataPath + "\\BattleState.xml");
    }

    [MenuItem(SAVE_MENU + "Player Data")]
    public static void OpenPlayerData()
    {
        Application.OpenURL(Application.persistentDataPath + "\\PlayerData.xml");
    }

    [MenuItem(SAVE_MENU + "Wipe Save Data")]
    public static void WipeSaveData()
    {
        if (EditorUtility.DisplayDialog("Confirm Deletion", "Delete BattleData.xml, PlayerData.xml and Map.json?", "Yes", "Cancel"))
        {
            if (File.Exists(PlayerSaveManager.FILE_PATH)) File.Delete(PlayerSaveManager.FILE_PATH);
            if (File.Exists(BattleStateManager.FILE_PATH)) File.Delete(BattleStateManager.FILE_PATH);
            if (File.Exists(Map.MapManager.FILE_PATH)) File.Delete(Map.MapManager.FILE_PATH);
            EditorUtility.DisplayDialog("Done!", "Deletion Completion", "Neat!");
        }
    }
}