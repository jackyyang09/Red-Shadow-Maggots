using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBenderSpyPassive : BaseCharacterPassive
{
    [Header("Wait Limit Effect")]
    [SerializeField] EffectStrength waitLimitStrength;
    [SerializeField] StatChangeEffect waitLimitEffect;
    [SerializeField] int waitLimitDuration;

    [Header("Status Effect")]
    [SerializeField] EffectStrength statusStrength;
    [SerializeField] EarthBenderSpyEffect statusEffect;

    bool qteSuccess;

    protected override void OnEnable()
    {
        base.OnEnable();

        baseCharacter.OnExecuteAttack += OnExecuteAttack;
        baseCharacter.OnDealDamage += OnDealDamage;
        baseCharacter.OnTakeDamage += OnTakeDamage;
        BattleSystem.OnEndTurn += OnEndTurn;
    }

    private void OnDisable()
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
    }

    void OnDealDamage(BaseCharacter character)
    {
        var props = new EffectProperties();

        props.effect = waitLimitEffect;
        props.effectDuration = waitLimitDuration;
        props.strength = EffectStrength.Medium;

        ApplyEffectToCharacter(character, props);
    }

    void OnTakeDamage(float trueDamage, DamageStruct damage)
    {
        if (damage.QTEResult == QuickTimeBase.QTEResult.Perfect)
        {
            qteSuccess = true;
        }
    }

    void OnEndTurn()
    {
        if (!qteSuccess) return;

        if (!baseCharacter.EffectDictionary.ContainsKey(statusEffect))
        {
            ApplyEffect(statusEffect, 1);
        }

        qteSuccess = false;
    }
}