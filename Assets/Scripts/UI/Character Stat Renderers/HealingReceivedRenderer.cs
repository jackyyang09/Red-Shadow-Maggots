using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingReceivedRenderer : BaseStatRenderer
{
    public override void RenderInBattle(BaseCharacter character)
    {
        var modifier = character.HealInModifier;
        if (modifier > 1)
        {
            ValueText = BeginColourLabel(controller.PositiveColor) + modifier.FormatPercentage() + END_COLOUR;
        }
        else if (modifier < 1)
        {
            ValueText = BeginColourLabel(controller.NegativeColor) + modifier.FormatPercentage() + END_COLOUR;
        }
        else
        {
            ValueText = modifier.FormatPercentage();
        }
    }
}
