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
        description.text = effect.description + " (";
        if (effect.remainingActivations > 0)
        {
            description.text += effect.remainingActivations + " Time";
            if (effect.remainingActivations > 1) description.text += "s";
        }
        if (effect.remainingTurns > 0)
        {
            if (effect.remainingActivations > 0) description.text += ", ";
            description.text += effect.remainingTurns + " Turn";
            if (effect.remainingTurns > 1) description.text += "s";
        }
        description.text += ")";
        canvas.Show();
    }

    public void Hide()
    {
        canvas.Hide();
    }
}
