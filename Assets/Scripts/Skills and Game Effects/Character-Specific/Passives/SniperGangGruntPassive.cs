using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class SniperGangGruntPassive : BaseCharacterPassive
{
    [SerializeField] EffectProperties crippleProps;
    [SerializeField] EffectProperties sparePartsProps;
    [SerializeField] EffectProperties waitLimitProps;

    protected override void Init()
    {
        baseCharacter.OnDealDamage += OnDealDamage;
        BaseCharacter.OnAppliedEffect += OnAppliedEffect;
    }

    protected override void Cleanup()
    {
        baseCharacter.OnDealDamage -= OnDealDamage;
        BaseCharacter.OnAppliedEffect -= OnAppliedEffect;
    }

    private void OnAppliedEffect(BaseCharacter character, AppliedEffect effect)
    {
        if (effect.referenceEffect == crippleProps.effect)
        {
            // Don't apply if you're allies
            if (character.IsPlayer() == baseCharacter.IsPlayer()) return;

            var stacks = character.EffectDictionary[effect.referenceEffect][0].Stacks;
            if (stacks == crippleProps.maxStacks)
            {
                ApplyEffectToCharacter(baseCharacter, sparePartsProps);
            }
        }
    }

    private void OnDealDamage(BaseCharacter character)
    {
        var target = battleSystem.OpposingCharacter;
        ApplyEffectToCharacter(target, crippleProps);
    }
}