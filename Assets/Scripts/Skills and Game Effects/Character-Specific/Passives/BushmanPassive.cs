using UnityEngine;
using static Facade;

public class BushmanPassive : BaseCharacterPassive
{
    [SerializeField] int stackRemovalOnFail;
    [SerializeField] int stackRemovalOnSuccess;
    [SerializeField] BushmanSupplies supplies;

    AppliedEffect SuppliesEffect => baseCharacter.EffectDictionary[supplies][0];

    protected override void Init()
    {
        baseCharacter.OnSkillUsed += AddStack;
        baseCharacter.OnTakeDamage += OnTakeDamage;
        baseCharacter.OnEndTurn += OnEndTurn;
    }

    protected override void Cleanup()
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
        if (HasStacks(supplies))
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