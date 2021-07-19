using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillNameUI : MonoBehaviour
{
    [SerializeField] float showTime = 2;

    [SerializeField] TextMeshProUGUI skillText = null;
    [SerializeField] OptimizedCanvas canvas = null;

    private void OnEnable()
    {
        GlobalEvents.OnCharacterActivateSkill += ShowSkillName;
    }

    private void OnDisable()
    {
        GlobalEvents.OnCharacterActivateSkill -= ShowSkillName;
    }

    private void ShowSkillName(BaseCharacter arg1, GameSkill arg2)
    {
        StartCoroutine(ShowSkillRoutine(arg2));
    }

    IEnumerator ShowSkillRoutine(GameSkill skill)
    {
        skillText.text = skill.referenceSkill.skillName;
        canvas.Show();

        yield return new WaitForSeconds(showTime);

        canvas.Hide();
    }
}
