using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Facade;
using System.Linq;

public class UICharacterDetails : BasicSingleton<UICharacterDetails>
{
    [SerializeField] float characterHoldTime = 0.75f;
    float fingerHoldTimer;

    [SerializeField] GameObject descriptionPrefab;
    [SerializeField] RectTransform contentRect;

    [SerializeField] OptimizedCanvas canvas;

    [SerializeField] JSAM.SoundFileObject panelOpenSound;
    [SerializeField] JSAM.SoundFileObject panelCloseSound;

    [SerializeField] CharacterHeadButton[] headButtons;

    List<UIStatusDescription> statusDescriptions = new List<UIStatusDescription>();

    BaseCharacter focusedCharacter;
    List<BaseCharacter> allyCharacters;

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
        focusedCharacter = character;

        UpdateEffectDescriptions();

        if (character.IsPlayer())
        {
            allyCharacters = battleSystem.PlayerList.ToList<BaseCharacter>();
        }
        else
        {
            allyCharacters = enemyController.EnemyList.ToList<BaseCharacter>();
        }

        int i;
        for (i = 0; i < allyCharacters.Count; i++)
        {
            headButtons[i].InitializeWithCharacter(allyCharacters[i], allyCharacters[i] == character);
        }

        for (; i < headButtons.Length; i++)
        {
            headButtons[i].gameObject.SetActive(false);
        }
    }

    public void TrySelectCharacter(BaseCharacter character)
    {
        if (focusedCharacter == character) return;

        focusedCharacter = character;
        CharacterPreviewUI.Instance.DisplayWithCharacter(focusedCharacter);
        UpdateEffectDescriptions();

        for (int i = 0; i < allyCharacters.Count; i++)
        {
            headButtons[i].SetSelected(allyCharacters[i] == character);
        }
    }

    void UpdateEffectDescriptions()
    {
        List<AppliedEffect> effects = new List<AppliedEffect>();
        foreach (var item in focusedCharacter.AppliedEffects)
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

        gameObject.RefreshLayoutGroupsImmediateAndRecursive(transform);
    }

    public void Hide()
    {
        //canvas.Hide();
    }
}