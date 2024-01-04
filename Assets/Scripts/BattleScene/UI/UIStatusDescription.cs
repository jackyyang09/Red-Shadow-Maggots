using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RSMConstants;

public class UIStatusDescription : MonoBehaviour
{
    [SerializeField] OptimizedCanvas canvas;

    [SerializeField] Image effectIcon;
    [SerializeField] TMPro.TextMeshProUGUI description;

    public void ApplyStatus(AppliedEffect effect)
    {
        effectIcon.sprite = effect.referenceEffect.effectIcon;

        switch (effect.referenceEffect.effectType)
        {
            case EffectType.None:
            default:
                description.color = Colours.CreamWhite;
                break;
            case EffectType.Heal:
            case EffectType.Buff:
                description.color = Colours.Buff;
                break;
            case EffectType.Debuff:
            case EffectType.Damage:
                description.color = Colours.Debuff;
                break;
        }
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