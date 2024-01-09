using DocumentFormat.OpenXml.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PisspenserPassive : BaseCharacterPassive
{
    [Header("Upgrades")]
    [SerializeField] PisspenserSpecial upgradeEffect;
    [SerializeField] PisspenserGlitch glitchEffect;

    [SerializeField] PeePoison peeEffect;

    int peesApplied = 0;

    bool IsGlitched => baseCharacter.EffectDictionary.ContainsKey(glitchEffect);
    bool HasUpgrades => baseCharacter.EffectDictionary.ContainsKey(upgradeEffect);
    AppliedEffect UpgradeEffect => baseCharacter.EffectDictionary[upgradeEffect][0];

    protected override void OnEnable()
    {
        base.OnEnable();

        baseCharacter.OnStartTurn += OnStartTurn;
        baseCharacter.OnTakeDamage += OnTakeDamage;
        BaseCharacter.OnAppliedEffect += OnAppliedEffect;
        BaseCharacter.OnRemoveEffect += OnRemoveEffect;
    }

    private void OnDisable()
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
                if (HasUpgrades)
                {
                    baseCharacter.RemoveEffect(UpgradeEffect, peesApplied);
                }
            }
            else
            {
                ApplyEffect(upgradeEffect, peesApplied);
                //ApplyEffect(upgradeEffect, 30);
            }
        }
    }

    void OnTakeDamage(float trueDamage, DamageStruct damage)
    {
        if (trueDamage > 0)
        {
            var props = new EffectProperties();
            props.effectDuration = -1;
            props.effect = glitchEffect;

            ApplyEffect(props);
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