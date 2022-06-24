using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCardHolder : MonoBehaviour
{
    [SerializeField] CharacterObject characterData = null;
    public CharacterObject Character
    {
        get { return characterData; }
    }

    [SerializeField] Rarity rarity;
    public Rarity Rarity
    {
        get { return rarity; }
    }

    [Header("Object References")]
    [SerializeField] Cinemachine.CinemachineVirtualCamera cam = null;
    [SerializeField] TMPro.TextMeshProUGUI nameText = null;

    [Header("Sprite References")]
    [SerializeField] Transform spriteHolder = null;
    [SerializeField] SpriteRenderer sprite = null;
    [SerializeField] Image[] classIcons = null;

    [Header("Rarity References")]
    [SerializeField] Material[] rarityMat = null;
    [SerializeField] Sprite[] rarityBackground = null;
    [SerializeField] Sprite[] rarityBorder = null;
    [SerializeField] Image[] stars = null;
    [SerializeField] Sprite[] classEmblemIcons = null;
    [SerializeField] Image classEmblem = null;
    [SerializeField] MeshRenderer cardMesh = null;
    [SerializeField] SpriteRenderer cardBorder = null;
    [SerializeField] SpriteRenderer cardBackground = null;
    [SerializeField] MeshRenderer backgroundRenderer = null;

    public System.Action<CharacterCardHolder> OnCardClicked;

    private void OnEnable()
    {
        if (characterData) ApplyCharacterChanges();
    }

    //private void OnDisable()
    //{
    //}

    public (CharacterObject character, Rarity rarity) GetHeldData()
    {
        return (characterData, rarity);
    }

    public void SetCharacterAndRarity(CharacterObject newRef, Rarity newRarity)
    {
        characterData = newRef;
        rarity = newRarity;

        if (spriteHolder.childCount > 0)
        {
            for (int i = spriteHolder.childCount - 1; i > -1; i--)
            {
                Destroy(spriteHolder.GetChild(i).gameObject);
            }
        }

        if (newRef.spriteObject)
        {
            Instantiate(newRef.spriteObject, spriteHolder).name = newRef.spriteObject.name;
        }
        else
        {
            sprite.sprite = newRef.sprite;
        }
        sprite.enabled = !newRef.spriteObject;

        nameText.text = characterData.characterName;

        cardMesh.material = rarityMat[(int)rarity];
        cardBackground.sprite = rarityBackground[(int)rarity];
        cardBorder.sprite = rarityBorder[(int)rarity];
        classEmblem.sprite = classEmblemIcons[(int)rarity];
        for (int i = 0; i < 3; i++)
        {
            classIcons[i].enabled = false;
        }
        classIcons[(int)characterData.characterClass].enabled = true;

        for (int i = 0; i < (int)Rarity.Count; i++)
        {
            stars[i].enabled = false;
        }
        for (int i = 0; i < (int)rarity + 1; i++)
        {
            stars[i].enabled = true;
        }
        if (backgroundRenderer) backgroundRenderer.material = rarityMat[(int)rarity];
    }

    [ContextMenu("Apply Reference and Rarity")]
    void ApplyCharacterChanges()
    {
        SetCharacterAndRarity(characterData, rarity);
    }

    public void SetPreviewCameraActive(bool active)
    {
        cam.enabled = active;
    }

    private void OnMouseDown()
    {
        OnCardClicked?.Invoke(this);

        if (CharacterPreviewUI.instance == null) return;
        if (!CharacterPreviewUI.instance.IsVisible && !CharacterPreviewUI.instance.IsCardLoadMode)
            PartyManager.OnSelectCharacter?.Invoke(this);
    }
}