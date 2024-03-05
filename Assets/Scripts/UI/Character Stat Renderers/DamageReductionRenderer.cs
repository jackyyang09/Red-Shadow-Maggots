using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReductionRenderer : BaseStatRenderer
{
    public override void RenderInBattle(BaseCharacter character)
    {
        var modifier = character.DamageAbsorptionModifier;
        if (modifier > 0)
        {
            ValueText += POS_COLOUR + modifier.FormatPercentage() + END_BRACKET;
        }
        else if (modifier < 0)
        {
            ValueText += POS_COLOUR + modifier.FormatPercentage() + END_BRACKET;
        }
        else
        {
            ValueText = 0f.FormatPercentage();
        }
    }
}
