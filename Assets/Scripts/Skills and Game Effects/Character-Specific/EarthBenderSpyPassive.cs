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

    bool HasStacks
    {
        get
        {
            if (baseCharacter.EffectDictionary.ContainsKey(statusEffect))
            {
                return baseCharacter.EffectDictionary[statusEffect].Count > 0;
            }
            return false;
        }
    }

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
            DecreaseStack();
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
        var qte = damage.QTEResult;

        switch (BattleSystem.Instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                qteSuccess = !baseCharacter.IsPlayer() && qte != QuickTimeBase.QTEResult.Perfect;
                break;
            case BattlePhases.EnemyTurn:
                qteSuccess = baseCharacter.IsPlayer() && qte == QuickTimeBase.QTEResult.Perfect;
                break;
        }

        if (!qteSuccess && damage.Source)
        {
            // Damage must be from an Attack
            RemoveEffect();
        }
    }

    void DecreaseStack()
    {
        if (!HasStacks) return;
        var ae = baseCharacter.EffectDictionary[statusEffect][0];
        baseCharacter.RemoveEffect(ae, 1);
    }

    void RemoveEffect()
    {
        if (!HasStacks) return;
        var ae = baseCharacter.EffectDictionary[statusEffect][0];
        baseCharacter.RemoveEffect(ae, ae.Stacks);
    }

    void OnEndTurn()
    {
        if (!qteSuccess) return;

        ApplyEffect(statusEffect, 1);

        qteSuccess = false;
    }
}