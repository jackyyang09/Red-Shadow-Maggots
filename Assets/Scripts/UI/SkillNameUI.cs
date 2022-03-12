using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SkillNameUI : BaseGameUI
{
    [SerializeField] float timeToHide = 2;
    [SerializeField] TextMeshProUGUI skillTypeText = null;
    [SerializeField] TextMeshProUGUI skillText = null;

    private void OnEnable()
    {
        BaseCharacter.OnCharacterActivateSkill += ShowSkillName;
        GlobalEvents.OnCharacterUseSuperCritical += ShowSuperCritName;
        SceneTweener.OnSkillUntween += Hide;
    }

    private void OnDisable()
    {
        BaseCharacter.OnCharacterActivateSkill -= ShowSkillName;
        GlobalEvents.OnCharacterUseSuperCritical -= ShowSuperCritName;
        SceneTweener.OnSkillUntween -= Hide;
    }

    private void ShowSkillName(BaseCharacter arg1, GameSkill arg2)
    {
        skillTypeText.text = "Skill";
        ShowPanel(arg2.referenceSkill.skillName);
    }

    private void ShowSuperCritName(BaseCharacter obj)
    {
        skillTypeText.text = "Super Critical";
        ShowPanel(obj.Reference.superCritical.skillName);
    }

    void ShowPanel(string skillName)
    {
        skillText.text = skillName;
        optimizedCanvas.Show();
        Invoke(nameof(Hide), timeToHide);
    }

    void Hide()
    {
        if (IsInvoking(nameof(Hide))) CancelInvoke(nameof(Hide));
        optimizedCanvas.Hide();
    }
}
