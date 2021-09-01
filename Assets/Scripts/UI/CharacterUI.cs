﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] protected Sprite[] classIcons = null;

    [SerializeField] protected Image icon = null;

    [SerializeField] protected RectTransform iconContainer = null;
    [SerializeField] protected List<Image> iconImages = new List<Image>();

    [SerializeField] protected List<BaseGameEffect> effects = new List<BaseGameEffect>();

    [SerializeField] protected BaseCharacter designatedCharacter = null;

    [SerializeField] protected Canvas parentCanvas;

    [ContextMenu("Find Object References")]
    void FindObjectReferences()
    {
        iconImages = new List<Image>(iconContainer.GetComponentsInChildren<Image>());

        designatedCharacter = GetComponentInParent<BaseCharacter>();
    }

    private void Start()
    {
        UpdateEffectIcons();
        SetClassIcon(designatedCharacter.Reference.characterClass);
    }

    private void OnEnable()
    {
        designatedCharacter.onApplyGameEffect += AddEffectIcon;
        designatedCharacter.onRemoveGameEffect += RemoveEffectIcon;

        GlobalEvents.OnEnterBattleCutscene += Hide;
        GlobalEvents.OnExitBattleCutscene += Show;
    }

    private void OnDisable()
    {
        designatedCharacter.onApplyGameEffect -= AddEffectIcon;
        designatedCharacter.onRemoveGameEffect -= RemoveEffectIcon;

        GlobalEvents.OnEnterBattleCutscene -= Hide;
        GlobalEvents.OnExitBattleCutscene -= Show;
    }

    void Show() => parentCanvas.enabled = true;
    void Hide() => parentCanvas.enabled = false;

    private void AddEffectIcon(BaseGameEffect obj)
    {
        effects.Add(obj);
        UpdateEffectIcons();
    }

    private void RemoveEffectIcon(BaseGameEffect obj)
    {
        effects.Remove(obj);
        UpdateEffectIcons();
    }

    void UpdateEffectIcons()
    {
        for (int i = 0; i < iconImages.Count; i++)
        {
            if (i >= effects.Count)
            {
                iconImages[i].enabled = false;
            }
            else
            {
                iconImages[i].enabled = true;
                iconImages[i].sprite = effects[i].effectIcon;
            }
        }
    }

    public void SetClassIcon(CharacterClass characterClass)
    {
        icon.sprite = classIcons[(int)characterClass];
    }
}
