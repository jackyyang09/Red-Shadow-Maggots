using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CharacterPreviewUI : MonoBehaviour
{
    [System.Serializable]
    public struct ClassTitleAndSprite
    {
        public string title;
        public Sprite icon;
    }

    [System.Serializable]
    public struct RarityTitleAndColor
    {
        public string title;
        public Color color;
    }

    [SerializeField] int selectedCharacterID = -1;

    [SerializeField] ClassTitleAndSprite[] classTuples = null;
    [SerializeField] RarityTitleAndColor[] rarityTuples = null;

    [Header("Object References")]

    [SerializeField] OptimizedCanvas canvas = null;
    public bool IsVisible
    {
        get { return canvas.IsVisible; }
    }

    [SerializeField] SkillDetailPanel skillPanel = null;

    [SerializeField] RectTransform readyRect = null;

    [SerializeField] TextMeshProUGUI nameText = null;
    [SerializeField] TextMeshProUGUI rarityText = null;
    [SerializeField] Image classImage = null;
    [SerializeField] TextMeshProUGUI classText = null;
    [SerializeField] SkillButtonUI skillButton1 = null;
    [SerializeField] SkillButtonUI skillButton2 = null;

    [SerializeField] TextMeshProUGUI healthText = null;
    [SerializeField] TextMeshProUGUI attackText = null;
    [SerializeField] TextMeshProUGUI critRateText = null;
    [SerializeField] TextMeshProUGUI critDamageText = null;
    [SerializeField] TextMeshProUGUI attackWindowText = null;
    [SerializeField] TextMeshProUGUI defenseWindowText = null;

    public static CharacterPreviewUI instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    private void OnEnable()
    {
        //PartyManager.OnSelectCharacter += BeginCharacterPreview;
    }

    private void OnDisable()
    {
        
    }

    public void BeginCharacterPreview(CharacterObject character, Rarity rarity)
    {
        if (!canvas.IsVisible)
        {
            SetReadyRectActive(false);
            UpdateCanvasProperties(character, rarity);
        }
    }

    void UpdateCanvasProperties(CharacterObject character, Rarity rarity)
    {
        var party = PartyManager.instance.CardHolders;
        CharacterCardHolder activeCharacter = null;
        for (int i = 0; i < party.Length; i++)
        {
            if (party[i].Character == character)
            {
                activeCharacter = party[i];
                selectedCharacterID = i;
            }
            party[i].SetPreviewCameraActive(false);
        }
        activeCharacter.SetPreviewCameraActive(true);

        canvas.Show();

        nameText.text = character.characterName;
        rarityText.text = rarityTuples[(int)rarity].title;
        rarityText.color = rarityTuples[(int)rarity].color;

        int classIndex = (int)character.characterClass;
        classImage.sprite = classTuples[classIndex].icon;
        classText.text = classTuples[classIndex].title;

        GameSkill newSkill = new GameSkill();
        newSkill.InitWithSkill(character.skills[0]);
        skillButton1.UpdateStatus(newSkill);
        newSkill.InitWithSkill(character.skills[1]);
        skillButton2.UpdateStatus(newSkill);

        float rarityMultiplier = 1 + 0.5f * (int)rarity;

        healthText.text = (character.maxHealth * rarityMultiplier).ToString();
        attackText.text = (character.attack * rarityMultiplier).ToString();
        critRateText.text = (character.critChance * 100).ToString() + "%";
        critDamageText.text = (character.critDamageMultiplier * 100).ToString() + "%";
        attackWindowText.text = character.attackLeniency.ToString();
        defenseWindowText.text = character.defenceLeniency.ToString();
    }

    public void ShowSkillDetailsWithID(int id)
    {
        var activeCharacter = PartyManager.instance.CardHolders[selectedCharacterID].Character;
        GameSkill newSkill = new GameSkill();
        newSkill.InitWithSkill(activeCharacter.skills[id]);
        skillPanel.UpdateDetails(newSkill);
        skillPanel.ShowPanel();
    }

    public void SetReadyRectActive(bool state)
    {
        switch (state)
        {
            case true:
                readyRect.DOAnchorPosY(0, 0.3f);
                break;
            case false:
                readyRect.DOAnchorPosY(-175, 0.3f);
                break;
        }
    }

    public void HideUI()
    {
        canvas.Hide();
        SetReadyRectActive(true);
        skillPanel.HidePanel();
        PartyManager.instance.CardHolders[selectedCharacterID].SetPreviewCameraActive(false);
    }

    bool cardLoadMode = false;
    public bool IsCardLoadMode
    {
        get { return cardLoadMode; }
    }

    public void EnterCardLoadMode()
    {
        cardLoadMode = true;
        SetReadyRectActive(false);
        CardLoader.instance.ShowFileDropOverlay();
    }

    public void ExitCardLoadMode()
    {
        cardLoadMode = false;
        SetReadyRectActive(true);
        CardLoader.instance.HideFileDropOverlay();
    }

    void EstablishSingletonDominance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // A unique case where the Singleton exists but not in this scene
            if (instance.gameObject.scene.name == null)
            {
                instance = this;
            }
            else if (!instance.gameObject.activeInHierarchy)
            {
                instance = this;
            }
            else if (instance.gameObject.scene.name != gameObject.scene.name)
            {
                instance = this;
            }
            Destroy(gameObject);
        }
    }
}