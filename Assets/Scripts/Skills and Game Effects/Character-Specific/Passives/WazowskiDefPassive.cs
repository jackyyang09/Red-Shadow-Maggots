using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WazowskiDefPassive : BaseCharacterPassive
{
    [Header("Door")]
    [SerializeField] EffectProperties doorProps;

    [SerializeField] EffectProperties shieldProps;

    protected override void Init()
    {
        baseCharacter.OnTakeDamage += OnTakeDamage;
    }

    protected override void Cleanup()
    {
        baseCharacter.OnTakeDamage -= OnTakeDamage;
    }

    private void OnTakeDamage(float trueDamage, DamageStruct damage)
    {
        if (!damage.Source) return;

        bool hadDoor = damage.Source.EffectDictionary.ContainsKey(doorProps.effect);

        ApplyEffectToCharacter(damage.Source, doorProps);

        if (!hadDoor) return;

        if (DefenseQTESuccessful(damage) && hadDoor)
        {
            ApplyEffect(shieldProps);
        }
    }
}