using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RSMConstants;

public class CharacterCardHolder : MonoBehaviour
{
    [SerializeField] CharacterObject characterData;
    public CharacterObject Character => characterData;

    [SerializeField] Rarity rarity;
    public Rarity Rarity => rarity;

    [Header("Object References")]
    [SerializeField] Cinemachine.CinemachineVirtualCamera cam;
    [SerializeField] TMPro.TextMeshProUGUI nameText;

    [Header("Stats References")]
    [SerializeField] OptimizedCanvas statsCanvas;
    public OptimizedCanvas StatsCanvas { get { return statsCanvas; } }
    [SerializeField] TMPro.TextMeshProUGUI levelLabel;
    [SerializeField] TMPro.TextMeshProUGUI healthLabel;
    [SerializeField] TMPro.TextMeshProUGUI maxHealthLabel;
    [SerializeField] Image healthFill;

    [Header("Sprite References")]
    [SerializeField] Transform spriteHolder;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Image[] classIcons;

    [Header("Rarity References")]
    [SerializeField] Material[] rarityMat;
    [SerializeField] Sprite[] rarityBackground;
    [SerializeField] Sprite[] rarityBorder;
    [SerializeField] Image[] stars;
    [SerializeField] Sprite[] classEmblemIcons;
    [SerializeField] Image classEmblem;
    [SerializeField] MeshRenderer cardMesh;
    [SerializeField] SpriteRenderer cardBorder;
    [SerializeField] SpriteRenderer cardBackground;
    [SerializeField] MeshRenderer backgroundRenderer;

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

    public void InitializeStatsCanvas(PlayerSave.MaggotState state)
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