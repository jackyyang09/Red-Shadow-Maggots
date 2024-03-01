using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Facade;
using Random = UnityEngine.Random;

public class PissPillPassive : BaseCharacterPassive
{
    [Header("Peed Application")]
    [SerializeField] EffectProperties peeProps;
    [SerializeField] float peeChance;
    [SerializeField] float peeChanceGreater;
    float[] peeChances;

    [Header("Debuff Clear")]
    [SerializeField] EffectProperties clearProps;
    [SerializeField] ClearDebuffs clearDebuffEffect;

    List<BaseCharacter> beneficiaries = new List<BaseCharacter>();

    protected override void Init()
    {
        peeChances = new float[] { peeChance, peeChanceGreater };

        baseCharacter.OnExecuteAttack += OnExecuteAttack;

        if (!baseCharacter.IsPlayer())
        {
            BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] += OnStartPhase;
        }
        else
        {
            BattleSystem.OnStartPhase[BattlePhases.EnemyTurn.ToInt()] += OnStartPhase;
        }

        BaseCharacter.OnCharacterDealDamage += OnDealDamage;
    }

    private void OnStartPhase()
    {
        beneficiaries.Clear();
    }

    protected override void Cleanup()
    {
        baseCharacter.OnExecuteAttack -= OnExecuteAttack;

        if (baseCharacter.IsPlayer())
        {
            BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] -= OnStartPhase;
        }
        else
        {
            BattleSystem.OnStartPhase[BattlePhases.EnemyTurn.ToInt()] -= OnStartPhase;
        }

        BaseCharacter.OnCharacterDealDamage += OnDealDamage;
    }

    private void OnExecuteAttack(bool qteSuccess)
    {
        float rngValue;

        rngValue = peeChances[qteSuccess.ToInt()];

        if (Random.value > rngValue) return;

        ApplyPeed();
    }

    public void ApplyPeed()
    {
        if (!battleSystem.OpposingCharacter.EffectDictionary.ContainsKey(peeProps.effect))
        {
            beneficiaries.Add(baseCharacter);
        }
        ApplyEffectToCharacter(battleSystem.OpposingCharacter, peeProps);
    }

    private void OnDealDamage(BaseCharacter attacker, BaseCharacter defender)
    {
        if (beneficiaries.Contains(attacker)) return;
        if (beneficiaries.Contains(defender)) return;

        // Trigger only when attacking an enemy with Peed
        if (!defender.EffectDictionary.ContainsKey(peeProps.effect)) return;

        List<BaseCharacter> allies = null;

        if (baseCharacter.IsPlayer() && battleSystem.CurrentPhase != BattlePhases.PlayerTurn) return;
        if (baseCharacter.IsPlayer())
        {
            if (battleSystem.CurrentPhase != BattlePhases.PlayerTurn) return;
            else
            {
                allies = battleSystem.LivingPlayers.Except(new List<BaseCharacter>() { attacker }).ToList();
            }
        }

        if (!baseCharacter.IsPlayer())
        {
            if (battleSystem.CurrentPhase != BattlePhases.EnemyTurn) return;
            else
            {
                allies = enemyController.LivingEnemies.Except(new List<BaseCharacter>() { attacker }).ToList();
            }
        }

        if (attacker.AppliedEffects.Any(e => e.referenceEffect.effectType == EffectType.Debuff))
        {
            ApplyEffect(clearProps);
        }
        else
        {
            allies.RemoveAll(a => !a.AppliedEffects.Any(e => e.referenceEffect.effectType == EffectType.Debuff));
            if (allies.Count > 0)
            {
                var randomAlly = allies[Random.Range(0, allies.Count)];
                ApplyEffectToCharacter(randomAlly, clearProps);
            }
        }

        beneficiaries.Add(attacker);
        beneficiaries.Add(defender);
    }
}