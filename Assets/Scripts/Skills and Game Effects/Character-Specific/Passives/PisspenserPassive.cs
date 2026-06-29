using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class PisspenserPassive : BaseCharacterPassive
{
    [Header("Upgrades")]
    [SerializeField] EffectProperties upgradeProps;
    [SerializeReference, SubclassSelector] BaseEffectTarget target;

    [SerializeField] EffectProperties glitchProps;

    [SerializeField] PeePoison peeEffect;

    bool IsGlitched => baseCharacter.EffectDictionary.ContainsKey(glitchProps.effect);
    AppliedEffect UpgradeEffect => baseCharacter.EffectDictionary[upgradeProps.effect][0];

    protected override void Init()
    {
        baseCharacter.OnStartTurn += OnStartTurn;
        baseCharacter.OnTakeDamage += OnTakeDamage;
    }

    protected override void Cleanup()
    {
        baseCharacter.OnStartTurn -= OnStartTurn;
        baseCharacter.OnTakeDamage -= OnTakeDamage;
    }

    void OnStartTurn()
    {
        var pees = 0;

        var everyone = battleSystem.AllCharacters;
        foreach (var e in everyone)
        {
            if (e.EffectDictionary.ContainsKey(peeEffect))
            {
                pees += e.EffectDictionary[peeEffect].Count;
            }
        }

        if (pees == 0) return;

        bool glitched = IsGlitched;

        var targets = target.GetTargets(baseCharacter, null);

        if (glitched) pees *= -1;
        upgradeProps.stacks = pees;

        foreach (var t in targets)
        {
            //if (t.EffectDictionary.ContainsKey(upgradeProps.effect))
            //{
            //    t.RemoveEffect(t.EffectDictionary[upgradeProps.effect][0]);
            //}
            ApplyEffectToCharacter(t, upgradeProps);
        }
    }

    void OnTakeDamage(float trueDamage, DamageStruct damage)
    {
        if (trueDamage > 0)
        {
            if (IsGlitched)
            {
                RemoveEffect(glitchProps.effect);
                Debug.Log("Remove Glitch!");
            }
            else
            {
                ApplyEffect(glitchProps);
                Debug.Log("Apply Glitch!");
            }
        }
    }
}