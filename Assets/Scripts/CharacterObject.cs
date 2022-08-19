using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using JSAM;

public enum AttackRange
{
    CloseRange,
    LongRange
}

public enum CharacterClass
{
    Offense,
    Defense,
    Support
}

public enum QTEType
{
    SimpleBar,
    Hold
}

[System.Serializable]
public struct AttackStruct
{
    public AnimationClip attackAnimation;
    public AttackRange attackRange;
    public bool isAOE;
}

#if UNITY_EDITOR
public class CharacterStatWizard : ScriptableWizard
{
    public Vector2 attackRange = new Vector2(1000, 10000);
    public Vector2 healthRange = new Vector2(1000, 10000);

    public Vector2 statPool = new Vector2(2000, 20000);

    public bool isEnemy;

    int level = 1;
    int attack = 1000;
    int health = 1000;

    Vector3 attackRangeMax;

    public CharacterObject[] targetCharacters;

    public static CharacterObject FocusedCharacter;

    public static void CreateWizard()
    {
        var w = ScriptableWizard.DisplayWizard<CharacterStatWizard>("Character Stat Wizard", "Apply", "Randomize w/ Class");
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

        if (isEnemy)
        {
            healthRange.y *= 100;
        }
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
        so.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(AttackStruct))]
public class AttackStructDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float spacing = -10;

        var thirdRect = new Rect(position);
        thirdRect.width = thirdRect.width / 3 - spacing;
        EditorGUI.PropertyField(thirdRect, property.FindPropertyRelative("attackAnimation"), GUIContent.none);
        thirdRect.position += new Vector2(thirdRect.width + spacing, 0);
        EditorGUI.PropertyField(thirdRect, property.FindPropertyRelative("attackRange"), GUIContent.none);
        thirdRect.position += new Vector2(thirdRect.width + spacing, 0);
        thirdRect.width /= 2;
        EditorGUI.LabelField(thirdRect, new GUIContent("Is AOE?"));
        thirdRect.position += new Vector2(thirdRect.width + spacing, 0);
        EditorGUI.PropertyField(thirdRect, property.FindPropertyRelative("isAOE"), GUIContent.none);
        EditorGUI.EndProperty();
    }
}
#endif

[CreateAssetMenu(fileName = "New Character", menuName = "ScriptableObjects/Character", order = 1)]
public class CharacterObject : ScriptableObject
{
    public string characterName = null;
    public Sprite sprite = null;
    public Sprite headshotSprite = null;

    [Header("Game Stats")]
    public const int MAX_LEVEL_PLAYER = 90;
    public const int MAX_LEVEL_ENEMY = 999;

    [SerializeField] Vector2 attackRange = new Vector2();
    [SerializeField] Vector2 healthRange = new Vector2();

    public int GetAttack(int currentLevel) => (int)Mathf.Lerp(attackRange.x, attackRange.y, (float)currentLevel / (float)MAX_LEVEL_PLAYER);
    public int GetMaxHealth(int currentLevel, bool isEnemy)
    {
        float lerp;
        if (!isEnemy) lerp = (float)currentLevel / (float)MAX_LEVEL_PLAYER;
        else
        {
            lerp = Mathf.Pow(currentLevel / (float)MAX_LEVEL_ENEMY, 2);
        }

        return (int)Mathf.Lerp(healthRange.x, healthRange.y, lerp);
    }

    float m = 0.7f;
    float z = 2.8f;
    float b = 0;

    public float GetExpRequiredForLevel(int currentLevel, int targetLevel)
    {
        if (currentLevel == 0)
        {
            return m * Mathf.Pow(targetLevel, z) + b;
        }
        else
        {
            return GetExpRequiredForLevel(0, targetLevel) - GetExpRequiredForLevel(0, currentLevel);
        }
    }

    public int GetLevelFromExp(float exp)
    {
        return (int)Mathf.Pow((exp - b) / m, 1 / z);
    }

    [Range(0, 1)] public float critChance;
    public float critDamageMultiplier = 3;
    public CharacterClass characterClass;
    public QTEType attackQteType;
    public int turnsToCrit = 3;

    [Range(0, 1)] public float attackLeniency;
    [Range(0, 1)] public float defenseLeniency;

    public SkillObject[] skills = null;

    public bool hasAltSkillAnimation = false;

    public SkillObject superCritical = null;

    [Header("Animation Properties")]
    public GameObject spriteObject = null;
    public AnimatorOverrideController animator = null;
    public GameObject characterRig = null;
    public AttackStruct[] attackAnimations;

    [Header("Effect Prefabs")]
    public GameObject attackEffectPrefab = null;
    public GameObject[] extraEffectPrefabs = null;

    [Header("Audio File Voice Objects")]
    public JSAMSoundFileObject voiceEntry = null;
    public JSAMSoundFileObject voiceAttack = null;
    public JSAMSoundFileObject voiceSelected = null;
    public JSAMSoundFileObject voiceFirstSkill = null;
    public JSAMSoundFileObject voiceSecondSkill = null;
    public JSAMSoundFileObject voiceHurt = null;
    public JSAMSoundFileObject voiceDeath = null;
    public JSAMSoundFileObject voiceVictory = null;

    [Header("Audio File Sound Objects")]
    public JSAMSoundFileObject weaponSound = null;

    public JSAMSoundFileObject[] extraSounds = null;

#if UNITY_EDITOR
    [ContextMenu(nameof(OpenStatWizard))]
    void OpenStatWizard()
    {
        CharacterStatWizard.CreateWizard();
        CharacterStatWizard.FocusedCharacter = this;
    }
#endif
}

public enum DamageEffectivess
{
    Resist,
    Normal,
    Effective
}

public static class DamageTriangle
{
    public const float RESIST = 0.75f;
    public const float NORMAL = 1;
    public const float EFFECTIVE = 1.25f;

    /// <summary>
    /// Offense = 0
    /// Defense = 0
    /// Support = 0
    /// Horizontal is attacker
    /// Vertical is defender
    /// </summary>
    public static float[,] matrix = new float[,]
    {            /*  Offense    Defense    Support  */
        /*Offense*/{ NORMAL   , RESIST   , EFFECTIVE },
        /*Defense*/{ EFFECTIVE, NORMAL   , RESIST    },
        /*Support*/{ RESIST   , EFFECTIVE, NORMAL    }
    };

    public static float GetEffectiveness(CharacterClass attacker, CharacterClass defender)
    {
        return matrix[(int)attacker, (int)defender];
    }

    public static DamageEffectivess EffectiveFloatToEnum(float val)
    {
        if (val == EFFECTIVE) return DamageEffectivess.Effective;
        else if (val == RESIST) return DamageEffectivess.Resist;
        else return DamageEffectivess.Normal;
    }
}