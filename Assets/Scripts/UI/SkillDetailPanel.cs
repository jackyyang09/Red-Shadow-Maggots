using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetailPanel : MonoBehaviour
{
    [SerializeField] Color buffColour = new Color(0.6f, 0.8f, 1);
    [SerializeField] Color debuffColour = new Color(1, 0.25f, 0.25f);
    string buffColourText, debuffColourText;

    [SerializeField] OptimizedCanvas canvas;
    [SerializeField] TMPro.TextMeshProUGUI nameText;
    [SerializeField] TMPro.TextMeshProUGUI cooldownCount;
    [SerializeField] TMPro.TextMeshProUGUI description;

    [Header("Explainers")]
    [SerializeField] RectTransform explainerParent;
    [SerializeField] GameObject effectExplainerPrefab;
    List<EffectExplainerUI> explainers = new List<EffectExplainerUI>();

    private void Start()
    {
        buffColourText = ColorUtility.ToHtmlStringRGB(buffColour);
        debuffColourText = ColorUtility.ToHtmlStringRGB(debuffColour);
    }

    private void OnEnable()
    {
        canvas.onCanvasShow.AddListener(PanelOpenSound);
        canvas.onCanvasHide.AddListener(PanelCloseSound);
    }

    private void OnDisable()
    {
        canvas.onCanvasShow.RemoveListener(PanelOpenSound);
        canvas.onCanvasHide.RemoveListener(PanelCloseSound);
    }

    void PanelOpenSound() => JSAM.AudioManager.PlaySound(BattleSceneSounds.UIPanelOpen);
    void PanelCloseSound() => JSAM.AudioManager.PlaySound(BattleSceneSounds.UIPanelClose);

    public void HidePanel()
    {
        canvas.Hide();

        foreach (var e in explainers)
        {
            e.HideUI();
        }
    }

    public void ShowWithDetails(GameSkill skill)
    {
        StartCoroutine(ShowRoutine(skill));
    }

    IEnumerator ShowRoutine(GameSkill skill)
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

        List<OptimizedCanvas> canvases = new List<OptimizedCanvas>();
        List<BaseGameEffect> effects = new List<BaseGameEffect>();

        int j = 0;
        for (int i = 0; j < skill.referenceSkill.gameEffects.Length; j++)
        {
            var e = skill.referenceSkill.gameEffects[j].effect;
            if (!e.IncludesExplainer) continue;
            if (effects.Contains(e)) continue;
            effects.Add(e);

            if (explainers.Count == i)
            {
                var explainer = Instantiate(effectExplainerPrefab, explainerParent).GetComponent<EffectExplainerUI>();
                explainers.Add(explainer);
            }
            explainers[i].InitializeWithEffect(e);
            canvases.Add(explainers[i].OptimizedCanvas);
            i++;
        }

        // Hide unused explainers
        for (; j < explainers.Count; j++)
        {
            if (explainers[j].OptimizedCanvas.IsVisible) break;
            explainers[j].HideUI();
        }

        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(explainerParent);

        yield return null;

        canvas.Show();

        foreach (var item in canvases)
        {
            item.Show();
        }
    }
}