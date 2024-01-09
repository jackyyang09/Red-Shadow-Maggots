using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHeadButton : MonoBehaviour
{
    [SerializeField] Color defaultColor;
    [SerializeField] Color selectedColor;

    [SerializeField] Image profileImage;
    [SerializeField] Image backgroundImage;
    [SerializeField] Image borderImage;

    [SerializeField] Material greyscaleMat;

    BaseCharacter character;

    public void InitializeWithCharacter(BaseCharacter c, bool selected)
    {
        character = c;

        gameObject.SetActive(true);
        profileImage.sprite = character.Reference.headshotSprite;
        profileImage.material = c.IsDead ? greyscaleMat : null;

        SetSelected(selected);
    }

    /// <summary>
    /// Invoked by a button
    /// </summary>
    void SelectThisCharacter()
    {
        UICharacterDetails.Instance.TrySelectCharacter(character);
    }

    public void SetSelected(bool selected)
    {
        var color = selected ? selectedColor : defaultColor;
        backgroundImage.color = color;
        borderImage.enabled = selected;
    }
}