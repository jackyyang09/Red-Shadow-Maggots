using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthShieldRenderer : StatRenderer
{
    [SerializeField] Image healthFill;
    [SerializeField] Image shieldFill;

    Material mat;

    private void Start()
    {
        // Instantiate material
        mat = healthFill.material;
    }

    public override void UpdateStat(BaseCharacter character)
    {
        mat.SetFloat("_Steps", character.MaxHealth / SimpleHealth.HEALTH_STEPS);

        healthFill.fillAmount = character.GetHealthPercent();
        shieldFill.fillAmount = character.ShieldPercent;
    }
}