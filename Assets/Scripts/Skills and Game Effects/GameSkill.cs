using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An instanced reference of a SkillObject that's possessed by a Character. 
/// Activates and applies AppliedEffects to targets upon use. 
/// </summary>
public class GameSkill
{
    /// <summary>
    /// How long to wait before skill recharges
    /// </summary>
    public int cooldownTimer = 0;
    public bool CanUse => cooldownTimer == 0;

    public SkillObject ReferenceSkill { get; private set; }

    public void InitWithSkill(SkillObject reference)
    {
        ReferenceSkill = reference;
    }

    public void BeginCooldown()
    {
        cooldownTimer = ReferenceSkill.coolDown;
    }

    public void Cooldown()
    {
        cooldownTimer = Mathf.Clamp(cooldownTimer - 1, 0, ReferenceSkill.coolDown);
    }
}