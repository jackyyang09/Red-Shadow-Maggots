using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpyPassive : BaseCharacterPassive
{
    //[SerializeField] EffectProperties tempBuffProps;
    [SerializeField] EffectProperties stackProps;

    //BaseCharacter lastTarget;
    //bool appliedBuff;

    protected override void Init()
    {
        //baseCharacter.OnBeginAttack += OnBeginAttack;
        baseCharacter.OnEndTurn += OnEndTurn;
        baseCharacter.OnTakeDamage += OnTakeDamage;
    }

    protected override void Cleanup()
    {
        //baseCharacter.OnBeginAttack -= OnBeginAttack;
        baseCharacter.OnEndTurn -= OnEndTurn;
        baseCharacter.OnTakeDamage -= OnTakeDamage;
    }

    //void OnBeginAttack(BaseCharacter target)
    //{
    //    if (lastTarget != target)
    //    {
    //        ApplyEffect(tempBuffProps);
    //        appliedBuff = true;
    //    }
    //    lastTarget = target;
    //}

    void OnEndTurn()
    {
        //if (appliedBuff)
        //{
        //    RemoveEffect(tempBuffProps.effect);
        //    appliedBuff = false;
        //}

        if (HasStacks(stackProps.effect))
        {
            var stackCount = GetStackCount(stackProps.effect);
            if (stackCount >= stackProps.maxStacks)
            {
                return;
            }
        }

        ApplyEffect(stackProps);
    }

    void OnTakeDamage(float trueDamage, DamageStruct damage)
    {
        if (trueDamage == 0) return;

        if (DefenseQTESuccessful(damage))
        {
            DecreaseStack(stackProps.effect);
        }
        else
        {
            RemoveEffect(stackProps.effect);
        }
    }
}