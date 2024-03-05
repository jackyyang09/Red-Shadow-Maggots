using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthRenderer : BaseStatRenderer
{
    public override void RenderInBattle(BaseCharacter character)
    {
        ValueText = Mathf.CeilToInt(character.CurrentHealth) + "/" + character.MaxHealth;
    }
}
