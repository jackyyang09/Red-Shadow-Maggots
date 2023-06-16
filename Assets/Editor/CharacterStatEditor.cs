using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

[System.Serializable]
public class ClassStats
{
    public float StatDeviancy;
    public float CritRate;
    public float CritDamage;
    public Vector2 DefenseRange;
}

[FilePath("ProjectSettings/CharacterStats.asset", FilePathAttribute.Location.ProjectFolder)]
public class CharacterStatEditor : ScriptableSingleton<CharacterStatEditor>
{
    public Vector2 StatPool = new Vector2(2000, 20000);
    public float EnemyHealthMultiplier = 3;
    public float BaseDeviancy = 0.05f;
    public ClassStats[] ClassStats;

    public string CharacterPath;
    public CharacterObject[] CharacterReferences;

    public void Save() => Save(true);
    public string GetPath() => GetFilePath();
}

public class CharacterStatWindow : EditorWindow
{
    public CharacterStatEditor Instance => CharacterStatEditor.instance;

    static CharacterStatWindow Window;

    [MenuItem(RSMEditorTools.RSM_TOOLS_MENU + "Character Stat Window")]
    public static void CreateWindow()
    {
        Window = GetWindow<CharacterStatWindow>();
    }

    private void OnEnable() 
    {
        if (Instance.ClassStats == null)
        {
            Instance.ClassStats = new ClassStats[3];
            Instance.ClassStats[(int)CharacterClass.Offense] = new ClassStats();
            Instance.ClassStats[(int)CharacterClass.Offense].StatDeviancy = 0.25f;
            Instance.ClassStats[(int)CharacterClass.Offense].CritRate = 0.01f;
            Instance.ClassStats[(int)CharacterClass.Offense].CritDamage = 0.5f;
            Instance.ClassStats[(int)CharacterClass.Offense].DefenseRange = new Vector2(0.1f, 0.2f);

            Instance.ClassStats[(int)CharacterClass.Defense] = new ClassStats();
            Instance.ClassStats[(int)CharacterClass.Defense].StatDeviancy = 0.15f;
            Instance.ClassStats[(int)CharacterClass.Defense].CritRate = 0.025f;
            Instance.ClassStats[(int)CharacterClass.Defense].CritDamage = 0.5f;
            Instance.ClassStats[(int)CharacterClass.Defense].DefenseRange = new Vector2(0.3f, 0.4f);

            Instance.ClassStats[(int)CharacterClass.Support] = new ClassStats();
            Instance.ClassStats[(int)CharacterClass.Support].StatDeviancy = 0.175f;
            Instance.ClassStats[(int)CharacterClass.Support].CritRate = 0.05f;
            Instance.ClassStats[(int)CharacterClass.Support].CritDamage = 0.5f;
            Instance.ClassStats[(int)CharacterClass.Support].DefenseRange = new Vector2(0.2f, 0.3f);
        }
    }

    private void OnGUI()
    {
        Undo.RecordObject(Instance, "Changed Character Stat Editor Props");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        Instance.CharacterPath = EditorGUILayout.TextField("Character Object Path", Instance.CharacterPath);
        if (Instance.CharacterPath == null) EditorGUILayout.LabelField("Loaded Characters", "0");
        else EditorGUILayout.LabelField("Loaded Characters", Instance.CharacterReferences.Length.ToString());
        if (GUILayout.Button("Load Characters At Path"))
        {
            Instance.CharacterReferences = EditorHelper.ImportAssetsOrFoldersAtPath<CharacterObject>(Instance.CharacterPath).ToArray();
        }
        EditorGUILayout.EndVertical();

        Instance.EnemyHealthMultiplier = EditorGUILayout.FloatField("Enemy Health Multiplier", Instance.EnemyHealthMultiplier);
        Instance.BaseDeviancy = EditorGUILayout.FloatField("Base Deviancy", Instance.BaseDeviancy);
        Instance.StatPool = EditorGUILayout.Vector2Field("Stat Pool Range", Instance.StatPool);

        for (int i = 0; i < Instance.ClassStats.Length; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(((CharacterClass)i).ToString(), TF2Ls.TF2LsStyles.CenteredLabel);
            Instance.ClassStats[i].StatDeviancy = EditorGUILayout.Slider("Stat Deviancy", Instance.ClassStats[i].StatDeviancy, 0, 1);
            Instance.ClassStats[i].CritRate = EditorGUILayout.FloatField("Crit Rate", Instance.ClassStats[i].CritRate);
            Instance.ClassStats[i].CritDamage = EditorGUILayout.FloatField("Crit Damage", Instance.ClassStats[i].CritDamage);
            Instance.ClassStats[i].DefenseRange = EditorGUILayout.Vector2Field("Defense Range", Instance.ClassStats[i].DefenseRange);
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Apply New Stats to Characters"))
        {
            RandomizeStatsForAllCharacters();
            EditorUtility.DisplayDialog("Character Stat Window", "Stats Applied", "OK");
        }

        if (GUILayout.Button("Save Settings"))
        {
            Instance.Save();
            EditorUtility.DisplayDialog("Character Stat Window", "Save Confirmed", "OK");
        }
    }

    float RandomDeviancy => Random.Range(-Instance.BaseDeviancy, Instance.BaseDeviancy);

    void RandomizeStatsForAllCharacters()
    {
        foreach (var character in Instance.CharacterReferences)
        {
            Vector2 attackRange;
            Vector2 defenseRange;
            Vector2 healthRange;
            Vector2 enemyHealthRange;
            float attackDeviancy = 0;
            var characterClass = (int)character.characterClass;

            var stats = Instance.ClassStats[characterClass];

            attackDeviancy = stats.StatDeviancy + RandomDeviancy;

            float healthDeviancy = 1 - attackDeviancy;

            attackRange.x = (int)Mathf.LerpUnclamped(0, Instance.StatPool.x, attackDeviancy);
            attackRange.y = (int)Mathf.LerpUnclamped(0, Instance.StatPool.y, attackDeviancy);

            healthRange.y = (int)Mathf.LerpUnclamped(0, Instance.StatPool.y, healthDeviancy);
            healthRange.x = (int)Mathf.LerpUnclamped(0, Instance.StatPool.x, healthDeviancy);

            defenseRange.x = Instance.ClassStats[characterClass].DefenseRange.x + RandomDeviancy;
            defenseRange.y = Instance.ClassStats[characterClass].DefenseRange.y + RandomDeviancy;

            enemyHealthRange.x = healthRange.x * Instance.EnemyHealthMultiplier;
            enemyHealthRange.y = healthRange.y * Instance.EnemyHealthMultiplier;

            SerializedObject so = new SerializedObject(character);
            so.FindProperty(nameof(attackRange)).vector2Value = attackRange;
            so.FindProperty(nameof(healthRange)).vector2Value = healthRange;
            so.FindProperty(nameof(enemyHealthRange)).vector2Value = enemyHealthRange;
            so.FindProperty("critChance").floatValue = Instance.ClassStats[characterClass].CritRate;
            so.FindProperty("critDamageMultiplier").floatValue = Instance.ClassStats[characterClass].CritDamage;
            so.FindProperty("defenseRange").vector2Value = defenseRange;
            so.ApplyModifiedProperties();
        }
    }
}