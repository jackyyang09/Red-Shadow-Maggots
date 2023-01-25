using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetailPanel : MonoBehaviour
{
    [SerializeField] Color buffColour = new Color(0.6f, 0.8f, 1);
    [SerializeField] Color debuffColour = new Color(1, 0.25f, 0.25f);
    string buffColourText, debuffColourText;

    [SerializeField] float minimumHeight = 500;
    [SerializeField] float lineHeight = 120;
    [SerializeField] int characterCountPerLine;

    [SerializeField] OptimizedCanvas canvas = null;
    [SerializeField] TMPro.TextMeshProUGUI nameText = null;
    [SerializeField] TMPro.TextMeshProUGUI cooldownCount = null;
    [SerializeField] TMPro.TextMeshProUGUI description = null;

    private void OnEnable()
    {
        canvas.onCanvasShow.AddListener(PanelOpenSound);
        canvas.onCanvasHide.AddListener(PanelCloseSound);
    }

    private void Start()
    {
        buffColourText = ColorUtility.ToHtmlStringRGB(buffColour);
        debuffColourText = ColorUtility.ToHtmlStringRGB(debuffColour);
    }

    void PanelOpenSound() => JSAM.AudioManager.PlaySound(BattleSceneSounds.UIPanelOpen);
    void PanelCloseSound() => JSAM.AudioManager.PlaySound(BattleSceneSounds.UIPanelClose);

    public void ShowPanel()
    {
        //Rebuild layout before showing
        StartCoroutine(RebuildAndShow());
    }

    public void HidePanel() => canvas.Hide();

    private void OnDisable()
    {
        canvas.onCanvasShow.RemoveListener(PanelOpenSound);
        canvas.onCanvasHide.RemoveListener(PanelCloseSound);
    }

    public void UpdateDetails(GameSkill skill)
    {
        nameText.text = skill.referenceSkill.skillName;
        cooldownCount.text = skill.referenceSkill.skillCooldown.ToString() + " Turn Cooldown";
        description.text = "";
        var descriptions = skill.referenceSkill.GetSkillDescriptions();
        for (int i = 0; i < descriptions.Length; i++)
        {
            description.text += "<color=#";
            switch (skill.referenceSkill.gameEffects[i].effect.effectType)
            {
                case EffectType.None:
                    description.text += ColorUtility.ToHtmlStringRGB(Color.grey);
                    break;
                case EffectType.Heal:
                case EffectType.Buff:
                    description.text += buffColourText;
                    break;
                case EffectType.Debuff:
                case EffectType.Damage:
                    description.text += debuffColourText;
                    break;
            }

            description.text += ">" + descriptions[i] + "</color>\n";
        }
    }

    // Rebuild the layout of the panel using LayoutRebuilder
    private IEnumerator RebuildAndShow()
    {
        yield return new WaitForEndOfFrame();
        var rect = GetComponent<RectTransform>();

        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        canvas.Show();
    }
}