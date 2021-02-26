using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
    public OptimizedButton button = null;

    [SerializeField] Image skillIcon = null;
    [SerializeField] Image darkOut = null;

    [SerializeField] TMPro.TextMeshProUGUI cooldownText = null;

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
