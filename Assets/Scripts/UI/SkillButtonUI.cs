using System;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
    [SerializeField] Image skillIcon;
    [SerializeField] Image darkOut;

    [SerializeField] TMPro.TextMeshProUGUI cooldownText;
    [SerializeField] private HoldButton _holdButton;

    public GameSkill currentSkill { get; private set; }


    public void UpdateStatus(GameSkill skill)
    {
        currentSkill = skill;
        skillIcon.sprite = skill.ReferenceSkill.sprite;

        var coolDown = skill.EffectiveCooldown?.Invoke();
        cooldownText.text = coolDown.ToString();

        cooldownText.enabled = !skill.CanUse;
        darkOut.enabled = !skill.CanUse;
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