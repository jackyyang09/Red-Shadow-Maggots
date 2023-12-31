using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStatusDescription : MonoBehaviour
{
    [SerializeField] OptimizedCanvas canvas;

    [SerializeField] Image effectIcon;
    [SerializeField] TMPro.TextMeshProUGUI description;

    public void ApplyStatus(AppliedEffect effect)
    {
        effectIcon.sprite = effect.referenceEffect.effectIcon;
        
        description.text = effect.description + " " + 
                BaseGameEffect.DurationAndActivationDescriptor(effect.remainingTurns, effect.remainingActivations);

        canvas.Show();

        StartCoroutine(DelayedRebuild());
    }

    /// <summary>
    /// Changing the description label takes too long and ruins layout. 
    /// A rebuild is necessary at this stage
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayedRebuild()
    {
        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
    }

    public void Hide()
    {
        canvas.Hide();
    }
}