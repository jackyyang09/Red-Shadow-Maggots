using UnityEngine;
using static Facade;

public class BushmanPassive : BaseCharacterPassive
{
    [SerializeField] int stackRemovalOnFail;
    [SerializeField] int stackRemovalOnSuccess;
    [SerializeField] BushmanSupplies supplies;

    bool HasStacks => baseCharacter.EffectDictionary.ContainsKey(supplies);
    AppliedEffect SuppliesEffect => baseCharacter.EffectDictionary[supplies][0];

    protected override void OnEnable()
    {
        base.OnEnable();

        baseCharacter.OnSkillUsed += AddStack;
        baseCharacter.OnTakeDamage += OnTakeDamage;
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

    private void OnEndTurn()
    {
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