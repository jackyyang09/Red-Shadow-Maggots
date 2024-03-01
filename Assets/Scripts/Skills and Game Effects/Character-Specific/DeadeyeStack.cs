using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dualist Passive Stack", menuName = "ScriptableObjects/Character-Specific/Dualist Stacks", order = 1)]
public class DeadeyeStack : BaseGameEffect, IStackableEffect
{
    public int MaxStacks => throw new System.NotImplementedException();

    public override string GetEffectDescription(AppliedEffect effect)
    {
        return "Increase ATK";
    }

    public void OnStacksChanged(AppliedEffect effect)
    {
        throw new System.NotImplementedException();
    }
}