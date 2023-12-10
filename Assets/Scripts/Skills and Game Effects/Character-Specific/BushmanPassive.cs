using UnityEngine;
using static Facade;

public class BushmanPassive : BaseCharacterPassive
{
    [SerializeField] int stackRemovalOnFail;
    [SerializeField] int stackRemovalOnSuccess;
    [SerializeField] BaseStackEffect supplies;

    [Header("Tier 1 - Heal per Turn")]
    [SerializeField] int healRequirement;
    [SerializeField] float healAmount = 0.01f;

    [Header("Tier 3 - Crit Chance Buff")]
    [SerializeField] int critChanceRequirement;
    [SerializeField] float critChanceModifier = 0.3f;

    [Header("Tier 4 - Dodge")]
    [SerializeField] int dodgeRequirement;
    [SerializeField] DodgeEffect dodgeEffect;

    bool HasStacks => baseCharacter.EffectDictionary.ContainsKey(supplies);
    AppliedEffect SuppliesEffect => baseCharacter.EffectDictionary[supplies][0];

    bool critChanceChanged;

    protected override void OnEnable()
    {
        base.OnEnable();
        baseCharacter.OnSkillUsed += AddStack;
        baseCharacter.OnTakeDamage += OnTakeDamage;
        // Add a character-specific event
        baseCharacter.OnStartTurn += OnStartTurn;
        baseCharacter.OnEndTurn += OnEndTurn;
    }

    protected void OnDisable()
    {
        baseCharacter.OnSkillUsed -= AddStack;
        baseCharacter.OnTakeDamage -= OnTakeDamage;
        baseCharacter.OnEndTurn -= OnEndTurn;
    }

    private void AddStack()
    {
        ApplyEffect(supplies, 1);
    }

    private void OnStartTurn()
    {
        if (HasStacks)
        {
            baseCharacter.Heal(baseCharacter.MaxHealth * SuppliesEffect.stacks * healAmount);

            SuppliesEffect.description += "For every stack, every turn, heal " + Keywords.Short.HEALTH + " equal to " +
                    healAmount * 100f + "% of your " + Keywords.Short.MAX_HEALTH;

            if (SuppliesEffect.stacks >= critChanceRequirement && !critChanceChanged)
            {
                critChanceChanged = true;
                baseCharacter.ApplyCritChanceModifier(critChanceModifier);
                SuppliesEffect.description += ". (Stacks >= 10) Gain 30% Crit Chance";
            }

            if (SuppliesEffect.stacks >= dodgeRequirement)
            {
                EffectProperties props = new EffectProperties() { effect = dodgeEffect, effectDuration = 1, activationLimit = 1 };
                ApplyEffect(props);
                SuppliesEffect.description += ". (Stacks >= 15) Every turn, apply Dodge (1 Time, 1 Turn).";
            }
        }
    }

    private void OnEndTurn()
    {
        if (battleSystem.ActivePlayer != baseCharacter) return;
        ApplyEffect(supplies, 30);
    }

    private void OnTakeDamage(float trueDamage, DamageStruct damage)
    {
        if (HasStacks)
        {
            var stacks = stackRemovalOnFail;
            if (damage.QTEResult == QuickTimeBase.QTEResult.Perfect)
            {
                stacks = stackRemovalOnSuccess;
            }
            baseCharacter.RemoveEffect(SuppliesEffect, stacks);
        }
    }
}