using UnityEngine;
using UnityEditor;

public class CharacterStatWizard : ScriptableWizard
{
    public Vector2 attackRange = new Vector2(1000, 10000);
    public Vector2 healthRange = new Vector2(1000, 10000);
    public Vector2 enemyHealthRange = new Vector2(1000, 100000);

    public Vector2 statPool = new Vector2(2000, 20000);

    public bool isEnemy;

    int level = 1;
    int attack = 1000;
    int health = 1000;

    Vector3 attackRangeMax;

    public CharacterObject[] targetCharacters;

    public static CharacterObject FocusedCharacter;

    [MenuItem(RSMEditorTools.RSM_TOOLS_MENU + "Character Stat Wizard")]
    public static void CreateWizard()
    {
        var w = ScriptableWizard.DisplayWizard<CharacterStatWizard>("Character Stat Wizard", "Apply", "Randomize w/ Class");
        if (Selection.activeObject)
        {
            FocusedCharacter = Selection.activeObject as CharacterObject;
        }
        w.OnWizardUpdate();
    }

    protected override bool DrawWizardGUI()
    {
        var modified = base.DrawWizardGUI();

        EditorGUI.BeginChangeCheck();
        level = EditorGUILayout.IntSlider("Level", level, 0, isEnemy ? CharacterObject.MAX_LEVEL_ENEMY : CharacterObject.MAX_LEVEL_PLAYER);

        using (new EditorGUI.DisabledGroupScope(true))
        {
            EditorGUILayout.IntSlider("Attack", attack, (int)attackRange.x, (int)attackRange.y);
            EditorGUILayout.IntSlider("Health", health, (int)healthRange.x, (int)healthRange.y);
        }

        if (GUILayout.Button("Copy Stats from Target"))
        {
            SerializedObject so = new SerializedObject(FocusedCharacter);
            attackRange = so.FindProperty(nameof(attackRange)).vector2Value;
            healthRange = so.FindProperty(nameof(healthRange)).vector2Value;
        }

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

    void RandomizeStats()
    {
        float baseDeviancy = 0.05f;
        float attackDeviancy = 0;

        //statPool.x += (int)(statPool.x * Random.Range(-baseDeviancy, baseDeviancy));
        //statPool.y += (int)(statPool.x * Random.Range(-baseDeviancy, baseDeviancy));

        switch (FocusedCharacter.characterClass)
        {
            case CharacterClass.Offense:
                attackDeviancy = 0.25f + Random.Range(-baseDeviancy, baseDeviancy);
                break;
            case CharacterClass.Defense:
                attackDeviancy = 0.15f + Random.Range(-baseDeviancy, baseDeviancy);
                break;
            case CharacterClass.Support:
                attackDeviancy = 0.175f + Random.Range(-baseDeviancy, baseDeviancy);
                break;
        }
        float healthDeviancy = 1 - attackDeviancy;

        attackRange.x = (int)Mathf.LerpUnclamped(0, statPool.x, attackDeviancy);
        healthRange.x = (int)Mathf.LerpUnclamped(0, statPool.x, healthDeviancy);
        attackRange.y = (int)Mathf.LerpUnclamped(0, statPool.y, attackDeviancy);
        healthRange.y = (int)Mathf.LerpUnclamped(0, statPool.y, healthDeviancy);

        enemyHealthRange.x = healthRange.x;
        enemyHealthRange.y = healthRange.y * 100;
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
        so.ApplyModifiedProperties();
    }
}