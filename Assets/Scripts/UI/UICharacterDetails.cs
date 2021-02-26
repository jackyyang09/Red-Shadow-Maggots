using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterDetails : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI nameText = null;
    [SerializeField] UnityEngine.UI.Image portrait = null;
    [SerializeField] TMPro.TextMeshProUGUI health = null;
    [SerializeField] TMPro.TextMeshProUGUI attack = null;
    [SerializeField] TMPro.TextMeshProUGUI critChance = null;
    [SerializeField] List<UIStatusDescription> statusDescriptions = null;
    [SerializeField] RectTransform contentRect = null;

    [SerializeField] OptimizedCanvas canvas = null;

    [SerializeField] Color positiveModifier = Color.yellow;
    [SerializeField] Color negativeModifier = Color.red;

    private void OnEnable()
    {
        canvas.onCanvasShow.AddListener(PanelOpenSound);
        canvas.onCanvasHide.AddListener(PanelCloseSound);
    }

    void PanelOpenSound() => JSAM.AudioManager.PlaySound(JSAM.Sounds.UIPanelOpen);
    void PanelCloseSound() => JSAM.AudioManager.PlaySound(JSAM.Sounds.UIPanelClose);

    private void OnDisable()
    {
        canvas.onCanvasShow.RemoveListener(PanelOpenSound);
        canvas.onCanvasHide.RemoveListener(PanelCloseSound);
    }

    public void DisplayWithCharacter(BaseCharacter character)
    {
        nameText.text = character.Reference.characterName;
        health.text = character.CurrentHealth + "/" + character.MaxHealth;

        portrait.sprite = character.Reference.headshotSprite;

        int modifiedAttack = (int)(character.Reference.attack * character.AttackModifier);
        attack.text = (character.Reference.attack + modifiedAttack).ToString();
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

        for (int i = 0; i < character.AppliedEffects.Count; i++)
        {
            statusDescriptions[i].ApplyStatus(character.AppliedEffects[i]);
        }
        contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, character.AppliedEffects.Count * 100);

        for (int i = character.AppliedEffects.Count; i < statusDescriptions.Count; i++)
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
