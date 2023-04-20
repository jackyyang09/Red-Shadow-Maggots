using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRenderer : BaseStatRenderer
{
    [SerializeField] TMPro.TextMeshProUGUI attackLabel;

    public override void UpdateRendererForCharacter(PlayerData.MaggotState state, CharacterObject character, bool isEnemy)
    {
        attackLabel.text = character.GetAttack(character.GetLevelFromExp(state.Exp)).ToString();
    }
}
