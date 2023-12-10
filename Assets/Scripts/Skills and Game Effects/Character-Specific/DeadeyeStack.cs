using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dualist Passive Stack", menuName = "ScriptableObjects/Character-Specific/Dualist Stacks", order = 1)]
public class DeadeyeStack : BaseStackEffect
{
    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        return "Increase ATK";
    }
}