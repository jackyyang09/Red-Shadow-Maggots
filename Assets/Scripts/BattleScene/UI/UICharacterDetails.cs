using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterDetails : BasicSingleton<UICharacterDetails>
{
    [SerializeField] Color positiveModifier = Color.yellow;
    [SerializeField] Color negativeModifier = Color.red;

    [SerializeField] TMPro.TextMeshProUGUI nameText;
    [SerializeField] TMPro.TextMeshProUGUI levelText;
    [SerializeField] UnityEngine.UI.Image portrait;
    [SerializeField] TMPro.TextMeshProUGUI health;
    [SerializeField] TMPro.TextMeshProUGUI attack;
    [SerializeField] TMPro.TextMeshProUGUI critChance;
    [SerializeField] GameObject descriptionPrefab;
    [SerializeField] List<UIStatusDescription> statusDescriptions;
    [SerializeField] RectTransform contentRect;

    [SerializeField] OptimizedCanvas canvas;

    [SerializeField] JSAM.SoundFileObject panelOpenSound;
    [SerializeField] JSAM.SoundFileObject panelCloseSound;

    private void OnEnable()
    {
        canvas.onCanvasShow.AddListener(PanelOpenSound);
        canvas.onCanvasHide.AddListener(PanelCloseSound);
    }

    void PanelOpenSound() => JSAM.AudioManager.PlaySound(panelOpenSound);
    void PanelCloseSound() => JSAM.AudioManager.PlaySound(panelCloseSound);

    private void OnDisable()
    {
        canvas.onCanvasShow.RemoveListener(PanelOpenSound);
        canvas.onCanvasHide.RemoveListener(PanelCloseSound);
    }

    public void DisplayWithCharacter(BaseCharacter character)
    {
        nameText.text = character.Reference.characterName;

        levelText.text = "Level " + character.CurrentLevel;

        health.text = character.CurrentHealth + "/" + character.MaxHealth;

        portrait.sprite = character.Reference.headshotSprite;

        int modifiedAttack = (int)(character.AttackModified - character.Attack);
        attack.text = (character.AttackModified).ToString();
        if (character.AttackModifier > 0)
        {
            attack.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(positiveModifier) + ">(+" + modifiedAttack + ")</color>";
        }
        else if (character.AttackModifier < 0)
        {
            attack.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(negativeModifier) + ">(" + modifiedAttack + ")</color>";
        }

        int modifiedCritChance = (int)(Mathf.Clamp01(character.CritChanceModifier) * 100);
        critChance.text = (character.Reference.critChance * 100 + modifiedCritChance).ToString() + "%";
        if (character.CritChanceModifier > 0)
        {
            critChance.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(positiveModifier) + ">(+" + modifiedCritChance + "%)</color>";
        }
        else if (character.CritChanceModifier < 0)
        {
            critChance.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(negativeModifier) + ">(" + modifiedCritChance + "%)</color>";
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

        contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, character.AppliedEffects.Count * 100);

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