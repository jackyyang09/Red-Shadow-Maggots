using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;

public enum AttackRange
{
    CloseRange,
    LongRange,
    AOE
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
    public string characterName = null;
    public Sprite sprite = null;
    public Sprite headshotSprite = null;

    [Header("Game Stats")]
    public float attack;
    public float maxHealth;
    [Range(0, 1)] public float critChance;
    public float critDamageMultiplier = 3;
    public CharacterClass characterClass;
    public AttackRange range;
    public QTEType attackQteType;
    public int turnsToCrit = 3;

    [Range(0, 1)] public float attackLeniency;
    [Range(0, 1)] public float defenseLeniency;

    public SkillObject[] skills = null;
    public SkillObject superCritical = null;

    [Header("Animation Properties")]
    public GameObject spriteObject = null;
    public AnimatorOverrideController animator = null;
    public GameObject characterRig = null;
    public AnimationClip attackAnimation = null;

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