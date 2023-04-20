using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritDamageRenderer : BaseStatRenderer
{
    [SerializeField] TMPro.TextMeshProUGUI critLabel;

    public override void UpdateRendererForCharacter(PlayerData.MaggotState state, CharacterObject character, bool isEnemy)
    {
        critLabel.text = (character.critDamageMultiplier * 100).ToString();
    }
}