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

        if (baseCharacter.IsPlayer() && battleSystem.CurrentPhase != BattlePhases.PlayerTurn) return;
        if (!baseCharacter.IsPlayer() && battleSystem.CurrentPhase != BattlePhases.EnemyTurn) return;

        if (attacker.AppliedEffects.Any(e => e.referenceEffect.effectType == EffectType.Debuff))
        {
            clearDebuffEffect.Activate(baseCharacter, attacker, EffectStrength.Medium, new float[0]);
        }

        beneficiaries.Add(attacker);
        beneficiaries.Add(defender);
    }
}