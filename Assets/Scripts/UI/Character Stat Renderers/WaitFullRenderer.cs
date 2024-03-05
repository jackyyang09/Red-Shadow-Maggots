using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitFullRenderer : BaseStatRenderer
{
    public override void RenderInBattle(BaseCharacter character)
    {
        ValueText = character.WaitTimer.FormatToDecimal() + "/" + character.WaitLimitModified.FormatToDecimal();
    }
}
