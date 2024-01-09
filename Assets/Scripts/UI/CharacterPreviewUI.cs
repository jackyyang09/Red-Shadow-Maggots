using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using RSMConstants;

public class CharacterPreviewUI : BasicSingleton<CharacterPreviewUI>
{
    [System.Serializable]
    public struct ClassTitleAndSprite
    {
        public string title;
        public Sprite icon;
    }

    [SerializeField] ClassTitleAndSprite[] classTuples;

    [Header("Object References")]

    [SerializeField] OptimizedCanvas canvas;
    public OptimizedCanvas OptimizedCanvas => canvas;
    public bool IsVisible => canvas.IsVisible;

    [SerializeField] StatRenderer[] statRenderers;

    [SerializeField] Button[] buttons;
    [SerializeField] OptimizedCanvas[] panels;
    int activePanel = -1;

    [SerializeField] SkillDetailPanel skillPanel;

    [SerializeField] SkillButtonUI skillButton1;
    [SerializeField] SkillButtonUI skillButton2;

    BaseCharacter previewedCharacter;
    CharacterObject previewedCharacterReference;

    public System.Action OnShow;
    public System.Action OnHide;

    private void Start()
    {
        SetActivePanel(0);
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        
    }

    public void DisplayWithCharacter(BaseCharacter character)
    {
        if (previewedCharacter)
        {
            previewedCharacter.AnimHelper.HideDetailsCam();
            OnHide -= previewedCharacter.AnimHelper.HideDetailsCam;
        }

        previewedCharacter = character;
        previewedCharacterReference = character.Reference;

        //if (rarityText)
        //{
        //    rarityText.text = rarity.ToTitle();
        //    rarityText.color = rarity.ToColour();
        //}

        //int classIndex = (int)character.characterClass;
        //if (classText) classText.text = classTuples[classIndex].title;

        GameSkill newSkill = new GameSkill();
        newSkill.InitWithSkill(previewedCharacterReference.skills[0]);
        skillButton1.UpdateStatus(newSkill);
        newSkill.InitWithSkill(previewedCharacterReference.skills[1]);
        skillButton2.UpdateStatus(newSkill);

        foreach (var item in statRenderers)
        {
            item.UpdateStat(character);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetRectTransform());

        previewedCharacter.AnimHelper.ShowDetailsCam();
        OnHide += previewedCharacter.AnimHelper.HideDetailsCam;

        OptimizedCanvas.Show();
        OnShow?.Invoke();
    }

    public void UpdateCanvasProperties(CharacterObject character, PlayerSave.MaggotState state, Rarity rarity)
    {
        previewedCharacterReference = character;

        //if (rarityText)
        //{
        //    rarityText.text = rarity.ToTitle();
        //    rarityText.color = rarity.ToColour();
        //}
        
        //int classIndex = (int)character.characterClass;
        //if (classText) classText.text = classTuples[classIndex].title;

        GameSkill newSkill = new GameSkill();
        newSkill.InitWithSkill(character.skills[0]);
        skillButton1.UpdateStatus(newSkill);
        newSkill.InitWithSkill(character.skills[1]);
        skillButton2.UpdateStatus(newSkill);

        foreach (var item in statRenderers)
        {
            item.UpdateStat(state, character, false);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetRectTransform());
        canvas.Show();
        OnShow?.Invoke();
    }

    public void SetActivePanel(int id)
    {
        if (activePanel == id) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].image.color = Colours.Button;
            panels[i].Hide();
        }

        activePanel = id;

        buttons[activePanel].image.color = Colours.ButtonSelected;
        panels[activePanel].Show();
    }

    public void ShowSkillDetailsWithID(int id)
    {
        GameSkill newSkill = new GameSkill();
        newSkill.InitWithSkill(previewedCharacterReference.skills[id]);
        skillPanel.ShowWithDetails(newSkill);
    }

    public void HideUI()
    {
        canvas.Hide();
        skillPanel.HidePanel();
        OnHide?.Invoke();
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