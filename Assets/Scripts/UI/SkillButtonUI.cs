using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
    public OptimizedButton button;

    [SerializeField] Image skillIcon;
    [SerializeField] Image darkOut;

    [SerializeField] TMPro.TextMeshProUGUI cooldownText;

    public void UpdateStatus(GameSkill skill)
    {
        skillIcon.sprite = skill.referenceSkill.sprite;
        if (!skill.CanUse)
        {
            cooldownText.enabled = true;
            cooldownText.text = skill.cooldownTimer.ToString();
            darkOut.enabled = true;
        }
        else
        {
            cooldownText.enabled = false;
            darkOut.enabled = false;
        }
    }
}
