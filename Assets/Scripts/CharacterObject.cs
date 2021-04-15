using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

[CreateAssetMenu(fileName = "New Character", menuName = "ScriptableObjects/Character", order = 1)]
public class CharacterObject : ScriptableObject
{
    public string characterName;
    public float attack;
    public float maxHealth;
    [Range(0, 1)] public float critChance;
    public float critDamageMultiplier;
    public Sprite sprite;
    public Sprite headshotSprite;
    public CharacterClass characterClass;
    public AttackRange range;
    public QTEType attackQteType;

    [Range(0, 1)] public float attackLeniency;
    [Range(0, 1)] public float defenceLeniency;

    public SkillObject[] skills;

    public GameObject spriteObject;
    public AnimatorOverrideController animator;

    public GameObject characterRig;

    [Header("Effect Prefabs")]
    public GameObject attackEffectPrefab;

    [Header("Audio File Voice Objects")]
    public AudioFileObject voiceEntry;
    public AudioFileObject voiceAttack;
    public AudioFileObject voiceSelected;
    public AudioFileObject voiceFirstSkill;
    public AudioFileObject voiceSecondSkill;
    public AudioFileObject voiceHurt;
    public AudioFileObject voiceDeath;
    public AudioFileObject voiceVictory;

    [Header("Audio File Sound Objects")]
    public AudioFileObject weaponSound;
}

public enum DamageEffectivess
{
    Normal,
    Resist,
    Effective
}

public static class DamageTriangle
{
    public const float EFFECTIVE = 1.25f;
    public const float RESIST = 0.75f;
    public const float NORMAL = 1;

    /// <summary>
    /// Offense = 0
    /// Defense = 0
    /// Support = 0
    /// Horizontal is attacker
    /// Vertical is defender
    /// </summary>
    public static float[,] matrix = new float[,]
    {   /*           Offense    Defense    Support */
        /*Offense*/{ NORMAL,    RESIST,    EFFECTIVE },
        /*Defense*/{ EFFECTIVE, NORMAL,    RESIST },
        /*Support*/{ RESIST,    EFFECTIVE, NORMAL }
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