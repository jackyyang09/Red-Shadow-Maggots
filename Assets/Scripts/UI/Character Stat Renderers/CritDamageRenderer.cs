using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritDamageRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = character.critDamageMultiplier.FormatPercentage();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        var modifiedCritDamage = character.CritDamageModifier.FormatPercentage();
        ValueText = character.CritDamageModified.FormatPercentage();
        if (character.CritDamageModifier > 0)
        {
            ValueText += RenderPositiveMod(modifiedCritDamage);
        }
        else if (character.CritDamageModifier < 0)
        {
            ValueText += RenderNegativeMod(modifiedCritDamage);
        }
    }
}
