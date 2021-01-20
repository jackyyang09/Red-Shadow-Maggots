using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStatusDescription : MonoBehaviour
{
    [SerializeField] OptimizedCanvas canvas;

    [SerializeField] UnityEngine.UI.Image effectIcon;
    [SerializeField] TMPro.TextMeshProUGUI description;

    private void OnValidate()
    {
        if (canvas == null) canvas = GetComponent<OptimizedCanvas>();
        if (effectIcon == null) effectIcon = GetComponentInChildren<UnityEngine.UI.Image>();
        if (description == null) description = GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }

    public void ApplyStatus(AppliedEffect effect)
    {
        effectIcon.sprite = effect.referenceEffect.effectIcon;
        description.text = effect.GetEffectDescription() + " (" + effect.remainingTurns;
        if (effect.remainingTurns == 1) description.text += " Turn)";
        else description.text += " Turns)";
        canvas.Show();
    }

    public void Hide()
    {
        canvas.Hide();
    }
}
