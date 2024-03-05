using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitGraphicRenderer : BaseStatRenderer
{
    [SerializeField] Image fillImage;

    public override void RenderInBattle(BaseCharacter character)
    {
        fillImage.fillAmount = character.WaitPercentage;
    }
}