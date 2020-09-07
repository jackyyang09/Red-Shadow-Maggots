using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave Object", menuName = "ScriptableObjects/Wave Object", order = 1)]
public class WaveObject : ScriptableObject
{
    public CharacterObject leftEnemy;
    public CharacterObject middleEnemy;
    public CharacterObject rightEnemy;
    public bool isBossWave;
}
