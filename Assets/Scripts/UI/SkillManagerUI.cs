using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManagerUI : MonoBehaviour
{
    [SerializeField] OptimizedCanvas canvas;

    /// <summary>
    /// The prefab for creating skill buttons.
    /// </summary>
    [SerializeField] private SkillButtonUI _skillButtonPrefab;

    /// <summary>
    /// The container for holding the spawned skill buttons.
    /// </summary>
    [SerializeField] private RectTransform _skillHolder;
    
    [SerializeField] private List<SkillButtonUI> buttons;

    /// <summary>
    /// The event that is invoked when a skill button is clicked.
    /// </summary>
    public static Action<GameSkill> OnSkillActivated { get; set; }

    /// <summary>
    /// The event that is invoked when a skill button is held.
    /// </summary>
    public Action<GameSkill> ShowDetails { get; set; }

    /// <summary>
    /// The event that is invoked when a skill button is released.
    /// </summary>
    public Action<GameSkill> HideDetails { get; set; }

    

    /// <summary>
    /// Initializes the SkillManagerUI by creating and setting up the specified number of skill buttons.
    /// </summary>
    public void Initialize()
    {
        SpawnButtons();
    }

    /// <summary>
    /// Spawns the specified number of skill buttons and adds them to the _skillHolder container.
    /// </summary>
    /// <param name="initialCount">The number of skill buttons to be spawned.</param>
    private void SpawnButtons()
    {
        foreach (var b in buttons)
        {
            b.SetListeners(SkillActivated, SkillHold, SkillRelease);
        }
    }

    public void ShowUI() => canvas.Show();
    public void HideUI() => canvas.Hide();

    /// <summary>
    /// Sets the specified skills to the skill buttons and updates their status.
    /// Automatically hides the skill buttons that are not used.
    /// </summary>
    /// <param name="skills">The skills to be set to the skill buttons.</param>
    public void SetSkills(List<GameSkill> skills)
    {
        for (var i = 0; i < buttons.Count; i++)
        {
            if (i < skills.Count)
            {
                buttons[i].UpdateStatus(skills[i]);
                buttons[i].gameObject.SetActive(true);
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Invokes the OnSkillActivated event with the specified skill.
    /// </summary>
    /// <param name="skill">The skill that was activated.</param>
    private void SkillActivated(GameSkill skill)
    {
        OnSkillActivated?.Invoke(skill);
    }

    /// <summary>
    /// Invokes the ShowDetails event with the specified skill.
    /// </summary>
    /// <param name="skill">The skill that was held.</param>
    private void SkillHold(GameSkill skill)
    {
        ShowDetails?.Invoke(skill);
    }

    /// <summary>
    /// Invokes the HideDetails event with the specified skill.
    /// </summary>
    /// <param name="skill">The skill that was released.</param>
    private void SkillRelease(GameSkill skill)
    {
        HideDetails?.Invoke(skill);
    }
}