using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class ClassStats
{
    public float StatDeviancy;
    public float CritRate;
    public float CritDamage;
}

[FilePath("ProjectSettings/CharacterStats.asset", FilePathAttribute.Location.ProjectFolder)]
public class CharacterStatEditor : ScriptableSingleton<CharacterStatEditor>
{
    public Vector2 StatPool = new Vector2(2000, 20000);
    public float EnemyHealthMultiplier = 3;
    public float BaseDeviancy = 0.05f;
}

public class CharacterStatWindow : EditorWindow
{
    static CharacterStatWindow Window;

    [MenuItem(RSMEditorTools.RSM_TOOLS_MENU + "Character Stat Window")]
    public static void CreateWindow()
    {
        Window = GetWindow<CharacterStatWindow>();
    }

    private void OnGUI()
    {
        EditorGUILayout.FloatField(CharacterStatEditor.instance.EnemyHealthMultiplier);
    }
}