using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackRange
{
    CloseRange,
    LongRange
}

[CreateAssetMenu(fileName = "New Character", menuName = "ScriptableObjects/Character Object", order = 1)]
public class CharacterObject : ScriptableObject
{
    public string characterName;
    public float attack;
    public float maxHealth;
    public Sprite sprite;
    public AttackRange range;
}