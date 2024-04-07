using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = character.wait.FormatToDecimal();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        var modifier = character.WaitModifier.FormatToDecimal();
        ValueText = character.WaitModified.FormatToDecimal();
        if (character.WaitModifier < 0)
        {
            ValueText += RenderPositiveMod(modifier);
        }
        else if (character.WaitModifier > 0)
        {
            ValueText += RenderNegativeMod(modifier, true);
        }
    }
}