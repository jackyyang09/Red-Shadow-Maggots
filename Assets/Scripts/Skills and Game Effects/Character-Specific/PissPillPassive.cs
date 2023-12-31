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
    [SerializeField] PeePoison peeEffect;
    [SerializeField] float peeChance;
    [SerializeField] float peeChanceGreater;
    [SerializeField] int peeDuration;
    float[] peeChances;

    [Header("Debuff Clear")]
    [SerializeField] ClearDebuffs clearDebuffEffect;

    List<BaseCharacter> beneficiaries = new List<BaseCharacter>();

    protected override void OnEnable()
    {
        base.OnEnable();

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

    private void OnDisable()
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
        var props = new EffectProperties();

        props.effect = peeEffect;
        props.effectDuration = peeDuration;
        props.strength = EffectStrength.Medium;

        if (!battleSystem.OpposingCharacter.EffectDictionary.ContainsKey(peeEffect))
        {
            beneficiaries.Add(baseCharacter);
        }
        ApplyEffectToCharacter(battleSystem.OpposingCharacter, props);
    }

    private void OnDealDamage(BaseCharacter attacker, BaseCharacter defender)
    {
        if (beneficiaries.Contains(attacker)) return;
        if (beneficiaries.Contains(defender)) return;

        // Trigger only when attacking an enemy with Peed
        if (!defender.EffectDictionary.ContainsKey(peeEffect)) return;

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
            clearDebuffEffect.Activate(baseCharacter, attacker, EffectStrength.Medium, new float[0]);
        }
        else
        {
            allies.RemoveAll(a => !a.AppliedEffects.Any(e => e.referenceEffect.effectType == EffectType.Debuff));
            if (allies.Count > 0)
            {
                var randomAlly = allies[Random.Range(0, allies.Count)];
                clearDebuffEffect.Activate(baseCharacter, randomAlly, EffectStrength.Medium, new float[0]);
            }
        }

        beneficiaries.Add(attacker);
        beneficiaries.Add(defender);
    }
}