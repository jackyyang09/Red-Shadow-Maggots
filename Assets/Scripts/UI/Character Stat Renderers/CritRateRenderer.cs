using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritRateRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = character.critChance.FormatPercentage();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        var modifiedCritChance = character.CritChanceModifier.FormatPercentage();
        ValueText = character.CritChanceModified.FormatPercentage();
        if (character.CritChanceModifier > 0)
        {
            ValueText += RenderPositiveMod(modifiedCritChance);
        }
        else if (character.CritChanceModifier < 0)
        {
            ValueText += RenderNegativeMod(modifiedCritChance);
        }
    }
}
