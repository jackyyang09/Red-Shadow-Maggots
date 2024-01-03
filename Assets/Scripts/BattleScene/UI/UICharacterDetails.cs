using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Facade;

public class UICharacterDetails : BasicSingleton<UICharacterDetails>
{
    [SerializeField] float characterHoldTime = 0.75f;
    float fingerHoldTimer;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] Image portrait;
    [SerializeField] TextMeshProUGUI health;
    [SerializeField] TextMeshProUGUI attack;
    [SerializeField] TextMeshProUGUI critChance;
    [SerializeField] GameObject descriptionPrefab;
    [SerializeField] RectTransform contentRect;
    [SerializeField] StatRenderer[] statRenderers;

    [SerializeField] OptimizedCanvas canvas;

    [SerializeField] JSAM.SoundFileObject panelOpenSound;
    [SerializeField] JSAM.SoundFileObject panelCloseSound;

    List<UIStatusDescription> statusDescriptions = new List<UIStatusDescription>();

    private void OnEnable()
    {
        BaseCharacter.OnMouseDragUpdate += OnMouseDragUpdate;
        BaseCharacter.OnMouseDragStop += OnMouseDragStop;

        canvas.onCanvasShow.AddListener(PanelOpenSound);
        canvas.onCanvasHide.AddListener(PanelCloseSound);
    }

    void PanelOpenSound() => JSAM.AudioManager.PlaySound(panelOpenSound);
    void PanelCloseSound() => JSAM.AudioManager.PlaySound(panelCloseSound);

    private void OnDisable()
    {
        BaseCharacter.OnMouseDragUpdate -= OnMouseDragUpdate;
        BaseCharacter.OnMouseDragStop -= OnMouseDragStop;

        canvas.onCanvasShow.RemoveListener(PanelOpenSound);
        canvas.onCanvasHide.RemoveListener(PanelCloseSound);
    }

    private void OnMouseDragUpdate(BaseCharacter c)
    {
        if (playerControlManager.CurrentMode >= PlayerControlMode.InCutscene) return;
        if (!UIManager.CanSelectCharacter) return;
        if (!ui.CharacterPanelOpen && !UIManager.SelectingAllyForSkill)
        {
            fingerHoldTimer += Time.deltaTime;
            if (fingerHoldTimer >= characterHoldTime)
            {
                ui.OpenCharacterPanel(c);
                fingerHoldTimer = 0;
            }
        }
    }

    void OnMouseDragStop(BaseCharacter c)
    {
        fingerHoldTimer = 0;
    }

    public void DisplayWithCharacter(BaseCharacter character)
    {
        nameText.text = character.Reference.characterName;

        levelText.text = "Level " + character.CurrentLevel;

        health.text = character.CurrentHealth + "/" + character.MaxHealth;

        portrait.sprite = character.Reference.headshotSprite;

        foreach (var stat in statRenderers)
        {
            stat.UpdateStat(character);
        }

        List<AppliedEffect> effects = new List<AppliedEffect>();
        foreach (var item in character.AppliedEffects)
        {
            effects.Add(item);
        }

        if (statusDescriptions.Count < effects.Count)
        {
            int diff = effects.Count - statusDescriptions.Count;
            for (int i = 0; i < diff; i++)
            {
                statusDescriptions.Add(Instantiate(descriptionPrefab, contentRect).GetComponent<UIStatusDescription>());
            }
        }

        for (int i = 0; i < effects.Count; i++)
        {
            statusDescriptions[i].ApplyStatus(effects[i]);
        }

        for (int i = effects.Count; i < statusDescriptions.Count; i++)
        {
            statusDescriptions[i].Hide();
        }

        canvas.Show();
    }

    public void Hide()
    {
        canvas.Hide();
    }
}