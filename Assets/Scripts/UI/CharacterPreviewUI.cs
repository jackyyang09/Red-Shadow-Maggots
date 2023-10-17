using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CharacterPreviewUI : BasicSingleton<CharacterPreviewUI>
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

    [SerializeField] ClassTitleAndSprite[] classTuples;
    [SerializeField] RarityTitleAndColor[] rarityTuples;

    [Header("Object References")]

    [SerializeField] OptimizedCanvas canvas;
    public OptimizedCanvas OptimizedCanvas { get { return canvas; } }
    public bool IsVisible
    {
        get { return canvas.IsVisible; }
    }

    [SerializeField] SkillDetailPanel skillPanel;

    [SerializeField] TextMeshProUGUI rarityText;
    [SerializeField] TextMeshProUGUI classText;
    [SerializeField] SkillButtonUI skillButton1;
    [SerializeField] SkillButtonUI skillButton2;

    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI critRateText;
    [SerializeField] TextMeshProUGUI critDamageText;
    [SerializeField] TextMeshProUGUI attackWindowText;
    [SerializeField] TextMeshProUGUI defenseWindowText;

    CharacterObject previewingCharacter;

    private void OnEnable()
    {
        //PartyManager.OnSelectCharacter += BeginCharacterPreview;
    }

    private void OnDisable()
    {
        
    }

    public void UpdateCanvasProperties(CharacterObject character, Rarity rarity)
    {
        previewingCharacter = character;

        if (rarityText)
        {
            rarityText.text = rarityTuples[(int)rarity].title;
            rarityText.color = rarityTuples[(int)rarity].color;
        }
        
        int classIndex = (int)character.characterClass;
        if (classText) classText.text = classTuples[classIndex].title;

        GameSkill newSkill = new GameSkill();
        newSkill.InitWithSkill(character.skills[0]);
        skillButton1.UpdateStatus(newSkill);
        newSkill.InitWithSkill(character.skills[1]);
        skillButton2.UpdateStatus(newSkill);

        healthText.text = Mathf.RoundToInt(character.GetMaxHealth(1, false)).ToString();
        attackText.text = Mathf.RoundToInt(character.GetAttack(1)).ToString();
        critRateText.text = (character.critChance * 100).ToString() + "%";
        critDamageText.text = (character.critDamageMultiplier * 100).ToString() + "%";
        attackWindowText.text = character.attackLeniency.ToString();
        defenseWindowText.text = character.defenseLeniency.ToString();
    }

    public void ShowSkillDetailsWithID(int id)
    {
        GameSkill newSkill = new GameSkill();
        newSkill.InitWithSkill(previewingCharacter.skills[id]);
        skillPanel.ShowWithDetails(newSkill);
    }

    public void ShowcaseCharacter()
    {
        ShowcaseSystem.Instance.ShowcaseCharacter(previewingCharacter);
    }

    public void HideUI()
    {
        canvas.Hide();
        skillPanel.HidePanel();
    }

    bool cardLoadMode = false;
    public bool IsCardLoadMode => cardLoadMode;

    public void EnterCardLoadMode()
    {
        cardLoadMode = true;
        CardLoader.instance.ShowFileDropOverlay();
    }

    public void ExitCardLoadMode()
    {
        cardLoadMode = false;
        CardLoader.instance.HideFileDropOverlay();
    }
}