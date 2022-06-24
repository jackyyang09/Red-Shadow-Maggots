using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Wave Object", menuName = "ScriptableObjects/Wave Object", order = 1)]
public class WaveObject : ScriptableObject
{
    public CharacterObject leftEnemy;
    public CharacterObject middleEnemy;
    public CharacterObject rightEnemy;
    public AssetReference[] Enemies;
    public bool IsBossWave;
    public bool UseSpecialCam;
}