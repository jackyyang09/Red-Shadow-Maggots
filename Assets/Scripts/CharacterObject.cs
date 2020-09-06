using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;

public enum AttackRange
{
    CloseRange,
    LongRange
}

public enum Class
{
    Offense,
    Defense,
    Support
}

[CreateAssetMenu(fileName = "New Character", menuName = "ScriptableObjects/Character", order = 1)]
public class CharacterObject : ScriptableObject
{
    public string characterName;
    public float attack;
    public float maxHealth;
    public Sprite sprite;
    public AttackRange range;
    [Range(0, 1)]
    public float attackLeniency;
    [Range(0, 1)]
    public float defenseLeniency;

    public SkillObject[] skills;

    [Header("Audio File Voice Objects")]
    public AudioFileObject voiceEntry;
    public AudioFileObject voiceAttack;
    public AudioFileObject voiceHurt;
    public AudioFileObject voiceDeath;
    public AudioFileObject voiceVictory;
}