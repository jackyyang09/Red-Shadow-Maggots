using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Wave Object", menuName = "ScriptableObjects/Wave Object", order = 1)]
public class WaveObject : ScriptableObject
{
    public AssetReference[] Enemies;
    public bool UseSpecialCam;
}