using System;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
    public OptimizedButton button = null;

    [SerializeField] Image skillIcon = null;
    [SerializeField] Image darkOut = null;

    [SerializeField] TMPro.TextMeshProUGUI cooldownText = null;
    [SerializeField] private HoldButton _holdButton;

    public GameSkill currentSkill { get; private set; }


    public void UpdateStatus(GameSkill skill)
    {
        currentSkill = skill;
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

    /// <summary>
    /// Set the listeners for the skill button
    /// </summary>
    /// </summary>
    /// <param name="onClick">Action to be invoked when button is clicked</param>
    /// <param name="onHold">Action to be invoked when button is held</param>
    /// <param name="onRelease">Action to be invoked when button is released</param>

    public void SetListeners(Action<GameSkill> onClick, Action<GameSkill> onHold, Action<GameSkill> onRelease)
    {
        _holdButton.onClick.RemoveAllListeners();
        _holdButton.onHoldEvent.RemoveAllListeners();
        _holdButton.onRelease.RemoveAllListeners();
        _holdButton.onClick.AddListener(() => onClick?.Invoke(currentSkill));
        _holdButton.onHoldEvent.AddListener(() => onHold?.Invoke(currentSkill));
        _holdButton.onRelease.AddListener(() => onRelease?.Invoke(currentSkill));
    }
}