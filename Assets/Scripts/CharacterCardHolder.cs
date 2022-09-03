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

    [Header("Stats References")]
    [SerializeField] OptimizedCanvas statsCanvas;
    public OptimizedCanvas StatsCanvas { get { return statsCanvas; } }
    [SerializeField] TMPro.TextMeshProUGUI levelLabel;
    [SerializeField] TMPro.TextMeshProUGUI healthLabel;
    [SerializeField] TMPro.TextMeshProUGUI maxHealthLabel;
    [SerializeField] Image healthFill;

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

    Animator spriteAnim;

    public System.Action<CharacterCardHolder> OnCardHovered;
    public System.Action<CharacterCardHolder> OnCardExited;
    public System.Action<CharacterCardHolder> OnCardClicked;

    private void OnEnable()
    {
        if (characterData) ApplyCharacterChanges();
    }

    //private void OnDisable()
    //{
    //}

    public void InitializeStatsCanvas(PlayerData.MaggotState state)
    {
        var level = characterData.GetLevelFromExp(state.Exp);
        levelLabel.text = level.ToString();
        healthLabel.text = ((int)state.Health).ToString();
        var maxHealth = characterData.GetMaxHealth(level, false);
        maxHealthLabel.text = maxHealth.ToString();
        healthFill.fillAmount = state.Health / maxHealth;
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
            var spriteInstance = Instantiate(newRef.spriteObject, spriteHolder);
            spriteInstance.name = newRef.spriteObject.name;
            spriteAnim = spriteInstance.GetComponent<Animator>();
            spriteAnim.SetTrigger("Pose");
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

        //if (CharacterPreviewUI.instance == null) return;
        //if (!CharacterPreviewUI.instance.IsVisible && !CharacterPreviewUI.instance.IsCardLoadMode)
        //    PartyManager.OnSelectCharacter?.Invoke(this);
    }

    private void OnMouseEnter()
    {
        if (spriteAnim) spriteAnim.SetBool("Pose", false);
        if (spriteAnim) spriteAnim.SetTrigger("Idle");

        OnCardHovered?.Invoke(this);
    }

    private void OnMouseExit()
    {
        if (spriteAnim) spriteAnim.SetBool("Pose", true);

        OnCardExited?.Invoke(this);
    }
}