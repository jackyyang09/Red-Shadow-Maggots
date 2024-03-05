using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = character.GetDefense(character.GetLevelFromExp(state.Exp)).FormatPercentage();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        var defenseModifier = character.DefenseModifier.FormatPercentage();
        ValueText = character.DefenseModified.FormatPercentage();
        if (character.DefenseModifier > 0)
        {
            ValueText += RenderPositiveMod(defenseModifier);
        }
        else if (character.DefenseModifier < 0)
        {
            ValueText += RenderNegativeMod(defenseModifier);
        }
    }
}
