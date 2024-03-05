using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitPercentRenderer : BaseStatRenderer
{
    public override void RenderInBattle(BaseCharacter character)
    {
        ValueText = character.WaitPercentage.FormatToDecimal() + "%";
    }
}