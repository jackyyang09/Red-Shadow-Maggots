using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EffectExplainerUI : BaseGameUI
{
    [SerializeField] TextMeshProUGUI titleLabel;
    [SerializeField] TextMeshProUGUI descLabel;

    public void InitializeWithEffect(BaseGameEffect effect)
    {
        titleLabel.text = effect.EffectName;
        descLabel.text = effect.EffectDescription;
    }

    public override void ShowUI()
    {
        optimizedCanvas.Show();
    }

    public override void HideUI()
    {
        optimizedCanvas.Hide();
    }
}