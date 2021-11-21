using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SkillNameUI : MonoBehaviour
{
    [SerializeField] float showTime = 2;

    [SerializeField] TextMeshProUGUI skillTypeText = null;
    [SerializeField] TextMeshProUGUI skillText = null;
    [SerializeField] OptimizedCanvas canvas = null;

    private void OnEnable()
    {
        GlobalEvents.OnCharacterActivateSkill += ShowSkillName;
        GlobalEvents.OnCharacterSuperCritical += ShowSuperCritName;
    }

    private void OnDisable()
    {
        GlobalEvents.OnCharacterActivateSkill -= ShowSkillName;
        GlobalEvents.OnCharacterSuperCritical -= ShowSuperCritName;
    }

    private void ShowSkillName(BaseCharacter arg1, GameSkill arg2)
    {
        skillTypeText.text = "Skill";
        StartCoroutine(ShowSkillRoutine(arg2.referenceSkill.skillName));
    }

    private void ShowSuperCritName(BaseCharacter obj)
    {
        skillTypeText.text = "Super Critical";
        StartCoroutine(ShowSkillRoutine(obj.Reference.superCritical.skillName));
    }

    IEnumerator ShowSkillRoutine(string skillName)
    {
        skillText.text = skillName;
        canvas.Show();

        yield return new WaitForSeconds(showTime);

        canvas.Hide();
    }
}
