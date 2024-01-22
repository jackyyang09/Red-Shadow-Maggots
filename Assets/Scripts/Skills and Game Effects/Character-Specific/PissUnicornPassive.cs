using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PissUnicornPassive : BaseCharacterPassive
{
    [SerializeField] float maxAttackBuff = 1.5f;
    [SerializeField] PeePoison peeEffect;
    [SerializeField] PissUnicornSpecial yellowState;

    float lastMod = 0;

    List<BaseCharacter> peedCharacters = new List<BaseCharacter>();

    bool SeeingYellow => baseCharacter.EffectDictionary.ContainsKey(yellowState);

    protected override void Init()
    {
        baseCharacter.OnHeal += OnHeal;
        baseCharacter.OnTakeDamage += OnTakeDamage;
        baseCharacter.onConsumeHealth += OnConsumeHealth;
        BaseCharacter.OnAppliedEffect += OnAppliedEffect;
        BaseCharacter.OnRemoveEffect += OnRemoveEffect;

        UpdateBerserkerState();
    }

    protected override void Cleanup()
    {
        baseCharacter.OnHeal -= OnHeal;
        baseCharacter.OnTakeDamage -= OnTakeDamage;
        baseCharacter.onConsumeHealth -= OnConsumeHealth;
        BaseCharacter.OnAppliedEffect -= OnAppliedEffect;
        BaseCharacter.OnRemoveEffect -= OnRemoveEffect;
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
        if (lastMod != 0) baseCharacter.ApplyAttackModifier(-lastMod);
        lastMod = (1 - baseCharacter.GetHealthPercent()) * maxAttackBuff;
        baseCharacter.ApplyAttackModifier(lastMod * baseCharacter.Attack);
    }

    void OnAppliedEffect(BaseCharacter character, AppliedEffect effect)
    {
        if (effect.referenceEffect != peeEffect) return;
        
        peedCharacters.Add(character);

        if (!SeeingYellow)
        {
            ApplyEffect(yellowState, 0);
        }
    }

    void OnRemoveEffect(BaseCharacter character, AppliedEffect effect)
    {
        if (effect.referenceEffect != peeEffect) return;

        if (!character.EffectDictionary.ContainsKey(peeEffect))
        {
            peedCharacters.Remove(character);
            if (peedCharacters.Count == 0 && SeeingYellow)
            {
                var e = baseCharacter.EffectDictionary[yellowState][0];
                baseCharacter.RemoveEffect(e);
            }
        }
    }
}