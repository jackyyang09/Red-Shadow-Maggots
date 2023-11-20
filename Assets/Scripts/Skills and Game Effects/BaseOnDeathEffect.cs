using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "On Death Effect", menuName = "ScriptableObjects/Game Stats/Defense", order = 1)]
public abstract class BaseOnDeathEffect : BaseGameEffect
{
    public abstract void OnDeath(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues);
}