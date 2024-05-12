using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBenderSpyPassive : BaseCharacterPassive
{
    [SerializeField] EffectProperties waitLimitProps;
    [SerializeField] EffectProperties statusProps;

    bool qteSuccess;

    protected override void Init()
    {
        baseCharacter.OnExecuteAttack += OnExecuteAttack;
        baseCharacter.OnDealDamage += OnDealDamage;
        baseCharacter.OnTakeDamage += OnTakeDamage;
        BattleSystem.OnEndTurn += OnEndTurn;
    }

    protected override void Cleanup()
    {
        baseCharacter.OnExecuteAttack -= OnExecuteAttack;
        baseCharacter.OnDealDamage -= OnDealDamage;
        baseCharacter.OnTakeDamage -= OnTakeDamage;
        BattleSystem.OnEndTurn -= OnEndTurn;
    }

    void OnExecuteAttack(bool b)
    {
        if (b)
        {
            qteSuccess = true;
        }
        else
        {
            DecreaseStack(statusProps.effect);
        }
    }

    void OnDealDamage(BaseCharacter character)
    {
        ApplyEffectToCharacter(character, waitLimitProps);
    }

    void OnTakeDamage(float trueDamage, DamageStruct damage)
    {
        var qteSuccess = DefenseQTESuccessful(damage);

        if (!qteSuccess && damage.Source)
        {
            // Damage must be from an Attack
            RemoveEffect(statusProps.effect);
        }
    }

    void OnEndTurn()
    {
        if (!qteSuccess) return;

        ApplyEffect(statusProps);

        qteSuccess = false;
    }
}