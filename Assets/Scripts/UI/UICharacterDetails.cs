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
    [SerializeField] GameObject descriptionPrefab = null;
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

    void PanelOpenSound() => JSAM.AudioManager.PlaySound(BattleSceneSounds.UIPanelOpen);
    void PanelCloseSound() => JSAM.AudioManager.PlaySound(BattleSceneSounds.UIPanelClose);

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

        List<AppliedEffect> effects = new List<AppliedEffect>();
        var keys = character.AppliedEffects.GetKeysCached();
        for (int i = 0; i < keys.Length; i++)
        {
            var list = character.AppliedEffects[keys[i]];
            for (int j = 0; j < list.Count; j++)
            {
                effects.Add(list[j]);
            }
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
