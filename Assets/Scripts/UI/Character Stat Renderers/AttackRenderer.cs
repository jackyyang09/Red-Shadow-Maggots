using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = character.GetAttack(character.GetLevelFromExp(state.Exp)).ToString();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        var attackModifier = Mathf.FloorToInt(character.AttackModifier);
        ValueText = Mathf.FloorToInt(character.AttackModified).ToString();
        if (attackModifier > 0)
        {
            ValueText += RenderPositiveMod(attackModifier);
        }
        else if (attackModifier < 0)
        {
            ValueText += RenderNegativeMod(attackModifier);
        }
    }
}
