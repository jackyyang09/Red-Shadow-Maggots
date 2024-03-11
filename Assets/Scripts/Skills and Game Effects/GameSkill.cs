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
    public int CooldownTimer;
    public bool CanUse => EffectiveCooldown?.Invoke() == 0;

    public delegate int CoolDownDelegate();
    // Because the real cooldown sometimes isn't what we're looking for
    public CoolDownDelegate EffectiveCooldown { get; private set; }

    public BaseAbilityObject ReferenceSkill { get; private set; }

    public GameSkill(BaseAbilityObject skill, CoolDownDelegate d = null)
    {
        if (d == null)
        {
            EffectiveCooldown = () => CooldownTimer;
        }
        else EffectiveCooldown = d;

        ReferenceSkill = skill;
    }

    public void BeginCooldown()
    {
        CooldownTimer = ReferenceSkill.coolDown;
    }

    public void Cooldown()
    {
        CooldownTimer = Mathf.Clamp(CooldownTimer - 1, 0, ReferenceSkill.coolDown);
    }
}