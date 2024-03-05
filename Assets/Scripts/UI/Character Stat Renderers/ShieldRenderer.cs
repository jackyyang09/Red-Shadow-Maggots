using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldRenderer : BaseStatRenderer
{
    public override void RenderInBattle(BaseCharacter character)
    {
        ValueText = Mathf.FloorToInt(character.CurrentShield).ToString();
    }
}
