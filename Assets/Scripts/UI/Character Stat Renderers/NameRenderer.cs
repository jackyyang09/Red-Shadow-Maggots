using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = character.characterName;
        RefreshValueLabelLayout();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        ValueText = character.Reference.characterName;
        RefreshValueLabelLayout();
    }
}