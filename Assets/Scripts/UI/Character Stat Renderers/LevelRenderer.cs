using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = "Lvl. " + character.GetLevelFromExp(state.Exp);
        RefreshValueLabelLayout();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        ValueText = "Lvl. " + character.CurrentLevel;
        RefreshValueLabelLayout();
    }
}
