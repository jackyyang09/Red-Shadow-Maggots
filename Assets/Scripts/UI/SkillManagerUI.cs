using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManagerUI : MonoBehaviour
{
    /// <summary>
    /// The prefab for creating skill buttons.
    /// </summary>
    [SerializeField] private SkillButtonUI _skillButtonPrefab;

    /// <summary>
    /// The container for holding the spawned skill buttons.
    /// </summary>
    [SerializeField] private RectTransform _skillHolder;
    
    /// <summary>
    /// The list that keeps track of the spawned skill buttons.
    /// We don't actually need this. We can just use the children of the skill holder instead.
    /// But yeah. I think this is better.
    /// </summary>
    private List<SkillButtonUI> _spawnedButtons;

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
    /// <param name="numberOfSkills">The number of skill buttons to be created and set up.</param>
    public void Initialize(int numberOfSkills)
    {
        _spawnedButtons = new List<SkillButtonUI>();

        DestroyAllButtons();
        SpawnButtons(numberOfSkills);
    }

    /// <summary>
    /// Destroys all skill buttons that are currently in the _skillHolder container
    /// </summary>
    private void DestroyAllButtons()
    {
        foreach (SkillButtonUI t in _skillHolder)
        {
            Destroy(t.gameObject);
        }
    }

    /// <summary>
    /// Spawns the specified number of skill buttons and adds them to the _skillHolder container.
    /// </summary>
    /// <param name="initialCount">The number of skill buttons to be spawned.</param>
    private void SpawnButtons(int initialCount)
    {
        for (var i = 0; i < initialCount; i++)
        {
            var skillButton = Instantiate(_skillButtonPrefab, _skillHolder);
            skillButton.SetListeners(SkillActivated, SkillHold, SkillRelease);
            _spawnedButtons.Add(skillButton);
        }
    }

    /// <summary>
    /// Hides all skill buttons. Duh
    /// </summary>
    public void HideAllButtons()
    {
        foreach (var spawnedButton in _spawnedButtons)
        {
            spawnedButton.button.Hide();
        }
    }

    /// <summary>
    /// Sets the specified skills to the skill buttons and updates their status.
    /// </summary>
    /// <param name="skills">The skills to be set to the skill buttons.</param>
    public void SetSkills(List<GameSkill> skills)
    {
        for (var i = 0; i < _spawnedButtons.Count; i++)
        {
            if (i < skills.Count)
            {
                _spawnedButtons[i].button.Show();
                _spawnedButtons[i].UpdateStatus(skills[i]);
            }
            else
            {
                _spawnedButtons[i].button.Hide();
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