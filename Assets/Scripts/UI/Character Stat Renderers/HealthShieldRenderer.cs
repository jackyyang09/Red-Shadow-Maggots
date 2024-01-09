using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthShieldRenderer : StatRenderer
{
    [SerializeField] Image healthFill;
    [SerializeField] Image shieldFill;

    public override void UpdateStat(BaseCharacter character)
    {
        healthFill.fillAmount = character.GetHealthPercent();
        shieldFill.fillAmount = character.ShieldPercent;
    }
}
