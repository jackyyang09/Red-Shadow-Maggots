using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField]
    Sprite[] classIcons;

    [SerializeField]
    Image icon;

    public void SetClassIcon(CharacterClass characterClass)
    {
        icon.sprite = classIcons[(int)characterClass];
    }
}
