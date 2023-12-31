using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameEffectIconUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI stackLabel;
    [SerializeField] Image image;
    AppliedEffect effect;

    public void InitializeWithEffect(AppliedEffect e)
    {
        effect = e;
        image.sprite = effect.referenceEffect.effectIcon;
        stackLabel.enabled = effect.HasStacks;
        UpdateStackCount();
    }

    public void UpdateStackCount()
    {
        stackLabel.text = effect.Stacks.ToString();
    }
}
