using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Defense Effect", menuName = "ScriptableObjects/Game Stats/Defense", order = 1)]
public abstract class BaseGameStat : ScriptableObject
{
    public virtual string Name => "";
    public abstract float GetGameStat(BaseCharacter target);
    public virtual void SetGameStat(BaseCharacter target, float value) { }
}