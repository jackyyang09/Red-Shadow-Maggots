using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif
using JSAM;

public enum AttackRange
{
    CloseRange,
    LongRange
}

public enum TweenBackType
{
    None,
    Teleport,
    Jump
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
[CustomPropertyDrawer(typeof(AttackStruct))]
public class AttackStructDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var thirdRect = new Rect(position);
        thirdRect.xMin = Mathf.Lerp(position.xMin, position.xMax, 0);
        thirdRect.xMax = Mathf.Lerp(position.xMin, position.xMax, 0.4f);
        EditorGUI.PropertyField(thirdRect, property.FindPropertyRelative("attackAnimation"), GUIContent.none);
        thirdRect.xMin = Mathf.Lerp(position.xMin, position.xMax, 0.425f);
        thirdRect.xMax = Mathf.Lerp(position.xMin, position.xMax, 0.725f);
        EditorGUI.PropertyField(thirdRect, property.FindPropertyRelative("attackRange"), GUIContent.none);
        thirdRect.xMin = Mathf.Lerp(position.xMin, position.xMax, 0.75f);
        thirdRect.xMax = Mathf.Lerp(position.xMin, position.xMax, 0.95f);
        EditorGUI.LabelField(thirdRect, new GUIContent("Is AOE?"));
        thirdRect.xMin = Mathf.Lerp(position.xMin, position.xMax, 0.95f);
        thirdRect.xMax = Mathf.Lerp(position.xMin, position.xMax, 1f);
        EditorGUI.PropertyField(thirdRect, property.FindPropertyRelative("isAOE"), GUIContent.none);
        EditorGUI.EndProperty();
    }
}
#endif

[CreateAssetMenu(fileName = "New Character", menuName = "ScriptableObjects/Character", order = 1)]
public class CharacterObject : ScriptableObject
{
    [ReadObjectName] public string characterName;
    public Sprite sprite;
    public Sprite headshotSprite;

    [Header("Game Stats")]
    public const int MAX_LEVEL_PLAYER = 90;
    public const int MAX_LEVEL_ENEMY = 999;

    [Header("Player Stats")]
    [SerializeField] Vector2 attackRange = new Vector2();
    [SerializeField] Vector2 healthRange = new Vector2();
    [SerializeField] Vector2 defenseRange = new Vector2();

    [Header("Enemy Stats")]
    [SerializeField] Vector2 enemyHealthRange = new Vector2();
    public bool isBoss;

    [ContextMenu(nameof(TestMaxHealth))]
    void TestMaxHealth()
    {
        Debug.Log(GetMaxHealth(0, false));
        Debug.Log(GetMaxHealth(1, false));
        Debug.Log(GetMaxHealth(2, false));
    }

    public int GetAttack(int currentLevel) => (int)Mathf.Lerp(attackRange.x, attackRange.y, (float)currentLevel / (float)MAX_LEVEL_PLAYER);
    public float GetDefense(int currentLevel) => Mathf.Lerp(defenseRange.x, defenseRange.y, (float)currentLevel / (float)MAX_LEVEL_PLAYER);
    public int GetMaxHealth(int currentLevel, bool isEnemy)
    {
        float lerp;
        if (!isEnemy)
        {
            lerp = (float)currentLevel / (float)MAX_LEVEL_PLAYER;
            return (int)Mathf.Lerp(healthRange.x, healthRange.y, lerp);
        }   
        else
        {
            lerp = (float)currentLevel / (float)MAX_LEVEL_PLAYER;
            //lerp = Mathf.Pow(currentLevel / (float)MAX_LEVEL_ENEMY, 2);
            return (int)Mathf.Lerp(enemyHealthRange.x, enemyHealthRange.y, lerp);
        }
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
    /// <summary>
    /// Time window in seconds in which the player can "perfect block" an attack
    /// </summary>
    [Range(0, 1)] public float defenseLeniency;

    public SkillObject[] skills;

    public bool hasAltSkillAnimation = false;

    public SkillObject superCritical;
    public AnimationClip superCriticalAnim;
    public bool isSuperCriticalAnAttack;
    public AttackRange superCritRange;
    public TweenBackType tweenBackStyle;

    [Header("Animation Properties")]
    public GameObject spriteObject;
    public AnimatorOverrideController animator;
    public AssetReference characterRig;
    public AttackStruct[] attackAnimations;
    public AttackStruct[] enemyAttackAnimations;

    [Header("Effect Prefabs")]
    public GameObject attackEffectPrefab;
    public GameObject[] projectileEffectPrefabs;
    public GameObject[] extraEffectPrefabs;

    public AssetReference environmentAsset;
    public AssetReference skyboxAsset;

    [Header("Audio File Voice Objects")]
    public SoundFileObject voiceEntry;
    public SoundFileObject voiceAttack;
    public SoundFileObject voiceSelected;
    public SoundFileObject voiceFirstSkill;
    public SoundFileObject voiceSecondSkill;
    public SoundFileObject voiceHurt;
    public SoundFileObject voiceDeath;
    public SoundFileObject voiceVictory;

    [Header("Audio File Sound Objects")]
    public SoundFileObject weaponSound;

    public SoundFileObject[] extraSounds;
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