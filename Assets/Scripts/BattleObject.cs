using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Battle Object", menuName = "ScriptableObjects/Battle Object", order = 1)]
public class BattleObject : ScriptableObject
{
    public List<WaveObject> waves;
}