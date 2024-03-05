using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseWindowRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = character.defenseLeniency.FormatPercentage();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        var modifier = character.DefenseLeniencyModifier.FormatPercentage();
        ValueText = character.DefenseLeniencyModified.FormatPercentage();
        if (character.DefenseLeniencyModifier > 0)
        {
            ValueText += RenderPositiveMod(modifier);
        }
        else if (character.DefenseLeniencyModifier < 0)
        {
            ValueText += RenderNegativeMod(modifier);
        }
    }
}