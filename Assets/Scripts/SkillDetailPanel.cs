using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDetailPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI nameText;
    [SerializeField] TMPro.TextMeshProUGUI cooldownCount;
    [SerializeField] TMPro.TextMeshProUGUI description;

    public void UpdateDetails(GameSkill skill)
    {
        nameText.text = skill.referenceSkill.skillName;
        cooldownCount.text = skill.referenceSkill.skillCooldown.ToString() + " Turn Cooldown";
        description.text = "\n";
        foreach (string line in skill.referenceSkill.skillDescription)
        {
            description.text = description.text.Insert(Mathf.Clamp(description.text.Length - 1, 0, description.text.Length), line + "\n");
        }
    }
}
