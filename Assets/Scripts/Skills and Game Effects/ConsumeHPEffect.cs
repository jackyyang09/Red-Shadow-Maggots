using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Consume HP Effect", menuName = "ScriptableObjects/Game Effects/Consume HP", order = 1)]
public class ConsumeHPEffect : BaseDamageEffect
{
    public override bool Activate(AppliedEffect effect)
    {
        base.ConsumeHealth(GetValue(stat, effect.values[0], effect.target), effect.target);

        return base.Activate(effect);
    }
}