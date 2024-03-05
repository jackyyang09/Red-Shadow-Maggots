using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitLimitRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = character.waitLimit.FormatToDecimal();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        var modifier = character.WaitLimitModifier.FormatToDecimal();
        ValueText = character.WaitLimitModified.FormatToDecimal();
        if (character.WaitLimitModifier > 0)
        {
            ValueText += RenderPositiveMod(modifier);
        }
        else if (character.WaitLimitModifier < 0)
        {
            ValueText += RenderNegativeMod(modifier);
        }
    }
}