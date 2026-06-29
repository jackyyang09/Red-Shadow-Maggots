using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAllyHelper : MonoBehaviour
{
    AppliedEffect effect;
    BattleAllyEffect battleAlly;
    public BaseCharacter Target;
    public bool AnimFinished = false;
    public float Attack;

    public void Initialize(AppliedEffect effect, BattleAllyEffect reference)
    {
        effect = this.effect;
        battleAlly = reference;
    }

    public void InvokeAttack()
    {
        battleAlly.InvokeAttack(this);
    }

    public void FinishAttack()
    {
        AnimFinished = true;
    }
}