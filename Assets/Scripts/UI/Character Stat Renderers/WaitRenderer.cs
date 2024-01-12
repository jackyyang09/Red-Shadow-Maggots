using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitRenderer : StatRenderer
{
    [SerializeField] Image fillImage;

    public override void UpdateStat(BaseCharacter character)
    {
        fillImage.fillAmount = character.WaitPercentage;
        RenderWaitPercent(character, Color.clear, Color.clear, null, valueLabel);
    }

    public override void UpdateStat(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
    }
}