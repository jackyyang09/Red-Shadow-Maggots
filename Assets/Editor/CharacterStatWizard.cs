using UnityEngine;
using UnityEditor;

public class CharacterStatWizard : ScriptableWizard
{
    public float enemyHealthMultiplier = 3f;
    public float baseDeviancy = 0.05f;

    public Vector2 statPool = new Vector2(2000, 20000);

    ClassStats[] classStats;

    public bool isEnemy;

    public CharacterObject[] targetCharacters;

    public static CharacterObject FocusedCharacter;

    const string CHARACTER_PATH = "Assets/ScriptableObjects/Character Data";

    [MenuItem(RSMEditorTools.RSM_TOOLS_MENU + "Character Stat Wizard")]
    public static void CreateWizard()
    {
        var w = ScriptableWizard.DisplayWizard<CharacterStatWizard>("Character Stat Wizard", "Apply", "Randomize w/ Class");
        if (Selection.activeObject)
        {
            FocusedCharacter = Selection.activeObject as CharacterObject;
        }
        w.OnWizardUpdate();

        w.classStats = new ClassStats[3];
        w.classStats[(int)CharacterClass.Offense] = new ClassStats();
        w.classStats[(int)CharacterClass.Offense].StatDeviancy = 0.25f;
        w.classStats[(int)CharacterClass.Offense].CritRate = 0.01f;
        w.classStats[(int)CharacterClass.Offense].CritDamage = 0.5f;

        w.classStats[(int)CharacterClass.Defense] = new ClassStats();
        w.classStats[(int)CharacterClass.Defense].StatDeviancy = 0.15f;
        w.classStats[(int)CharacterClass.Defense].CritRate = 0.025f;
        w.classStats[(int)CharacterClass.Defense].CritDamage = 0.5f;

        w.classStats[(int)CharacterClass.Support] = new ClassStats();
        w.classStats[(int)CharacterClass.Support].StatDeviancy = 0.175f;
        w.classStats[(int)CharacterClass.Support].CritRate = 0.05f;
        w.classStats[(int)CharacterClass.Support].CritDamage = 0.5f;
    }

    Vector2 attackRange;
    Vector2 healthRange;
    Vector2 enemyHealthRange;
    int level = 1;
    int attack = 1000;
    int health = 1000;

    protected override bool DrawWizardGUI()
    {
        var modified = base.DrawWizardGUI();

        if (GUILayout.Button("Get All Characters"))
        {
            var assets = AssetDatabase.FindAssets("t:CharacterObject", new string[] { CHARACTER_PATH });
            var newList = new System.Collections.Generic.List<CharacterObject>();
            foreach (var item in assets)
            {
                var co = AssetDatabase.LoadAssetAtPath<CharacterObject>(AssetDatabase.GUIDToAssetPath(item));
                if (co) newList.Add(co);
            }
            targetCharacters = newList.ToArray();
        }

        for (int i = 0; i < classStats.Length; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(((CharacterClass)i).ToString(), TF2Ls.TF2LsStyles.CenteredLabel);
            classStats[i].StatDeviancy = EditorGUILayout.Slider("Stat Deviancy", classStats[i].StatDeviancy, 0, 1);
            classStats[i].CritRate = EditorGUILayout.FloatField("Crit Rate", classStats[i].CritRate);
            classStats[i].CritDamage = EditorGUILayout.FloatField("Crit Damage", classStats[i].CritDamage);
            EditorGUILayout.EndVertical();
        }

        EditorGUI.BeginChangeCheck();
        level = EditorGUILayout.IntSlider("Level", level, 0, isEnemy ? CharacterObject.MAX_LEVEL_ENEMY : CharacterObject.MAX_LEVEL_PLAYER);

        using (new EditorGUI.DisabledGroupScope(true))
        {
            EditorGUILayout.IntSlider("Attack", attack, (int)attackRange.x, (int)attackRange.y);
            EditorGUILayout.IntSlider("Health", health, (int)healthRange.x, (int)healthRange.y);
        }

        //if (GUILayout.Button("Copy Stats from Target"))
        //{
        //    SerializedObject so = new SerializedObject(FocusedCharacter);
        //    attackRange = so.FindProperty(nameof(attackRange)).vector2Value;
        //    healthRange = so.FindProperty(nameof(healthRange)).vector2Value;
        //}

        if (GUILayout.Button("Apply Stats to Targets"))
        {
            foreach (var co in targetCharacters)
            {
                FocusedCharacter = co;
                RandomizeStats();
                ApplyStatsToCharacter();
            }
        }

        return modified || EditorGUI.EndChangeCheck();
    }

    void OnWizardCreate()
    {
        ApplyStatsToCharacter();
    }

    private void OnWizardOtherButton()
    {
        RandomizeStats();

        OnWizardUpdate();
    }

    void OnWizardUpdate()
    {
        float aLerp = (float)level / (float)CharacterObject.MAX_LEVEL_PLAYER;
        float hLerp = 0;
        if (!isEnemy)
        {
            hLerp = (float)level / (float)CharacterObject.MAX_LEVEL_PLAYER;
        }
        else
        {
            hLerp = Mathf.Pow(level / (float)CharacterObject.MAX_LEVEL_ENEMY, 2);
        }

        attack = (int)Mathf.Lerp(attackRange.x, attackRange.y, aLerp);
        health = (int)Mathf.Lerp(healthRange.x, healthRange.y, hLerp);
    }

    float RandomDeviancy => Random.Range(-baseDeviancy, baseDeviancy);

    void RandomizeStats()
    {
        float attackDeviancy = 0;

        var stats = classStats[(int)FocusedCharacter.characterClass];

        attackDeviancy = stats.StatDeviancy + RandomDeviancy;

        float healthDeviancy = 1 - attackDeviancy;

        attackRange.x = (int)Mathf.LerpUnclamped(0, statPool.x, attackDeviancy);
        healthRange.x = (int)Mathf.LerpUnclamped(0, statPool.x, healthDeviancy);
        attackRange.y = (int)Mathf.LerpUnclamped(0, statPool.y, attackDeviancy);
        healthRange.y = (int)Mathf.LerpUnclamped(0, statPool.y, healthDeviancy);

        enemyHealthRange.x = healthRange.x * enemyHealthMultiplier;
        enemyHealthRange.y = healthRange.y * enemyHealthMultiplier;
    }

    void ApplyStatsToCharacter()
    {
        if (FocusedCharacter == null)
        {
            if (FocusedCharacter == null) return;
        }
        SerializedObject so = new SerializedObject(FocusedCharacter);
        so.FindProperty(nameof(attackRange)).vector2Value = attackRange;
        so.FindProperty(nameof(healthRange)).vector2Value = healthRange;
        so.FindProperty(nameof(enemyHealthRange)).vector2Value = enemyHealthRange;
        so.FindProperty("critChance").floatValue = classStats[(int)FocusedCharacter.characterClass].CritRate;
        so.FindProperty("critDamageMultiplier").floatValue = classStats[(int)FocusedCharacter.characterClass].CritDamage;
        so.ApplyModifiedProperties();
    }
}