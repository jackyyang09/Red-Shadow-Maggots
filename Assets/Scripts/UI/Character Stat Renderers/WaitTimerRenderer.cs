using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitTimerRenderer : BaseStatRenderer
{
    public override void RenderInBattle(BaseCharacter character)
    {
        ValueText = character.WaitTimer.FormatToDecimal();
    }
}
