using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class DualistPassive : BaseCharacterPassive
{
    [SerializeField] int maxStacks = 6;
    [SerializeField] DeadeyeStack stackEffect;
    [SerializeField] DeadeyeSpecial specialEffect;

    protected override void Init()
    {
        baseCharacter.OnBeginAttack += AddStack;
        baseCharacter.OnExecuteAttack += OnExecuteAttack;
        // TODO: Add stacks at end of turn so the effect is visible
        baseCharacter.OnStartTurn += OnStartTurn;
    }

    protected override void Cleanup()
    {
        baseCharacter.OnBeginAttack -= AddStack;
        baseCharacter.OnExecuteAttack -= OnExecuteAttack;
        baseCharacter.OnStartTurn -= OnStartTurn;
    }

    private void OnStartTurn()
    {
        if (battleSystem.ActivePlayer != baseCharacter) return;
        if (baseCharacter.EffectDictionary.ContainsKey(stackEffect))
        {
            var eff = baseCharacter.EffectDictionary[stackEffect][0];
            if (eff.Stacks >= maxStacks)
            {
                baseCharacter.RemoveEffect(eff, maxStacks);
                ApplyEffect(specialEffect, 1);
                baseCharacter.EnhancedBasicAttack = true;
                baseCharacter.OnEndTurn += OnEndTurn;
            }
        }
    }

    private void OnEndTurn()
    {
        baseCharacter.OnEndTurn -= OnEndTurn;
        baseCharacter.RemoveEffect(baseCharacter.EffectDictionary[specialEffect][0]);
        baseCharacter.EnhancedBasicAttack = false;
    }

    private void OnExecuteAttack(bool qteSuccess)
    {
        if (!qteSuccess) return;
        AddStack(null);
    }

    private void AddStack(BaseCharacter target)
    {
        ApplyEffect(stackEffect, 1);
    }
}