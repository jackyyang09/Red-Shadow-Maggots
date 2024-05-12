using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class PisspenserPassive : BaseCharacterPassive
{
    [Header("Upgrades")]
    [SerializeField] EffectProperties upgradeProps;
    [SerializeField] EffectProperties glitchProps;

    [SerializeField] PeePoison peeEffect;

    int peesApplied = 0;

    bool IsGlitched => baseCharacter.EffectDictionary.ContainsKey(glitchProps.effect);
    AppliedEffect UpgradeEffect => baseCharacter.EffectDictionary[upgradeProps.effect][0];

    protected override void Init()
    {
        baseCharacter.OnStartTurn += OnStartTurn;
        baseCharacter.OnTakeDamage += OnTakeDamage;
        BaseCharacter.OnAppliedEffect += OnAppliedEffect;
        BaseCharacter.OnRemoveEffect += OnRemoveEffect;
    }

    protected override void Cleanup()
    {
        baseCharacter.OnStartTurn -= OnStartTurn;
        baseCharacter.OnTakeDamage -= OnTakeDamage;
        BaseCharacter.OnAppliedEffect -= OnAppliedEffect;
        BaseCharacter.OnRemoveEffect -= OnRemoveEffect;
    }

    void OnStartTurn()
    {
        if (peesApplied >= 0)
        {
            if (IsGlitched)
            {
                if (HasStacks(upgradeProps.effect))
                {
                    baseCharacter.RemoveEffect(UpgradeEffect, peesApplied);
                }
            }
            else if (peesApplied > 0)
            {
                var props = upgradeProps.Copy();
                props.stacks = peesApplied;
                ApplyEffect(props);
                //BaseCharacter.ApplyEffectToCharacter(props, baseCharacter, allies.ToArray());
                //ApplyEffect(upgradeEffect, 30);
            }
        }
    }

    void OnTakeDamage(float trueDamage, DamageStruct damage)
    {
        if (trueDamage > 0)
        {
            if (IsGlitched)
            {
                RemoveEffect(glitchProps.effect);
            }
            else
            {
                ApplyEffect(glitchProps);
            }
        }
    }

    void OnAppliedEffect(BaseCharacter character, AppliedEffect effect)
    {
        if (effect.referenceEffect != peeEffect) return;

        peesApplied++;
    }

    void OnRemoveEffect(BaseCharacter character, AppliedEffect effect)
    {
        if (effect.referenceEffect != peeEffect) return;

        peesApplied = Mathf.Max(peesApplied - 1, 0);
    }
}