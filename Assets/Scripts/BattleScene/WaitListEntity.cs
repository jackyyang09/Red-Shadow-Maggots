using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitListEntity
{
    public float Wait;
}

[CreateAssetMenu(fileName = "New Waitist Object", menuName = "ScriptableObjects/Wait List Object", order = 1)]
public class WaitListObject : ScriptableObject
{
    public string Name;
    public Sprite HeadshotSprite;
    public float Wait;
    public float WaitLimit;
}