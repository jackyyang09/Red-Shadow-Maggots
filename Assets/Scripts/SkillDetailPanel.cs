using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDetailPanel : MonoBehaviour
{
    [SerializeField] OptimizedCanvas canvas = null;
    [SerializeField] TMPro.TextMeshProUGUI nameText = null;
    [SerializeField] TMPro.TextMeshProUGUI cooldownCount = null;
    [SerializeField] TMPro.TextMeshProUGUI description = null;

    private void OnEnable()
    {
        canvas.onCanvasShow.AddListener(PanelOpenSound);
        canvas.onCanvasHide.AddListener(PanelCloseSound);
    }

    void PanelOpenSound() => JSAM.AudioManager.PlaySound(BattleSceneSounds.UIPanelOpen);
    void PanelCloseSound() => JSAM.AudioManager.PlaySound(BattleSceneSounds.UIPanelClose);

    public void ShowPanel() => canvas.Show();
    public void HidePanel() => canvas.Hide();

    private void OnDisable()
    {
        canvas.onCanvasShow.RemoveListener(PanelOpenSound);
        canvas.onCanvasHide.RemoveListener(PanelCloseSound);
    }

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