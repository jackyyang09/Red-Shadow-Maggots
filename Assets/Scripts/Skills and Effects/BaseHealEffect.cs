using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Heal Effect", menuName = "ScriptableObjects/Skills/Heal", order = 1)]
public class BaseHealEffect : BaseGameEffect
{
    public float healAmount;

    public override void Activate(List<BaseCharacter> targets)
    {
        foreach (BaseCharacter t in targets)
        {
            t.Heal(healAmount);
        }
    }

    public override void Tick()
    {
        throw new System.NotImplementedException();
    }
}
