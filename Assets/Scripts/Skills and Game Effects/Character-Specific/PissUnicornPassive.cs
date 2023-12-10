using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PissUnicornPassive : BaseCharacterPassive
{
    protected override void OnEnable()
    {
        base.OnEnable();

        baseCharacter.OnHeal += OnHeal;
        baseCharacter.OnTakeDamage += OnTakeDamage;
        baseCharacter.onConsumeHealth += OnConsumeHealth;

        UpdateBerserkerState();
    }

    private void OnDisable()
    {
        baseCharacter.OnHeal -= OnHeal;
        baseCharacter.OnTakeDamage -= OnTakeDamage;
        baseCharacter.onConsumeHealth -= OnConsumeHealth;
    }

    private void OnHeal()
    {
        UpdateBerserkerState();
    }

    private void OnTakeDamage(float trueDamage, DamageStruct damage)
    {
        UpdateBerserkerState();
    }

    private void OnConsumeHealth(float obj)
    {
        UpdateBerserkerState();
    }

    void UpdateBerserkerState()
    {

    }
}