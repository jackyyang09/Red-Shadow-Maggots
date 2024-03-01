using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dualist Passive Special", menuName = "ScriptableObjects/Character-Specific/Dualist Special", order = 1)]
public class DeadeyeSpecial : BaseGameEffect
{
    public override string GetEffectDescription(AppliedEffect effect)
    {
        return "Dutch's next attack is an AOE";
    }
}