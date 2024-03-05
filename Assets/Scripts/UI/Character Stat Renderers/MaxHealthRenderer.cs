using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxHealthRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = character.GetMaxHealth(character.GetLevelFromExp(state.Exp), isEnemy).ToString();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        ValueText = character.MaxHealth.ToString();
    }
}
