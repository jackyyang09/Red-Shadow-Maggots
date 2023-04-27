using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxHealthRenderer : BaseStatRenderer
{
    [SerializeField] TMPro.TextMeshProUGUI healthLabel;

    public override void UpdateRendererForCharacter(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        healthLabel.text = character.GetMaxHealth(character.GetLevelFromExp(state.Exp), isEnemy).ToString();
    }
}