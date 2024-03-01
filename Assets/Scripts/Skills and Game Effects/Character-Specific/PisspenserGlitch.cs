using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pisspenser Glitch", menuName = "ScriptableObjects/Character-Specific/Piss/Pisspenser Glitch", order = 1)]
public class PisspenserGlitch : BaseGameEffect
{
    public override string GetEffectDescription(AppliedEffect effect)
    {
        string d = "The Pisspenser is glitched, causing Upgrade stacks to be lost rather than gained.";
        return d;
    }
}