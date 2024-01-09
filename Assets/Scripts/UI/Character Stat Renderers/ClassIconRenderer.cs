using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassIconRenderer : StatRenderer
{
    [SerializeField] Sprite[] icons;
    [SerializeField] UnityEngine.UI.Image iconImage;

    public override void UpdateStat(BaseCharacter character)
    {
        iconImage.sprite = icons[(int)character.Reference.characterClass];
    }

    public override void UpdateStat(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        iconImage.sprite = icons[(int)character.characterClass];
    }
}