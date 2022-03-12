using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetMode
{
    None,
    OneAlly,
    OneEnemy,
    AllAllies,
    AllEnemies,
    Self
}

/// <summary>
/// The basic definition of a skill recorded in a CharacterObject
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "ScriptableObjects/Skill Object", order = 1)]
public class SkillObject : ScriptableObject
{
    [System.Serializable]
    public struct EffectProperties
    {
        public BaseGameEffect effect;
        public int effectDuration;
        public EffectStrength strength;
        public float[] customValues;
    }

    public string skillName;
    public List<string> skillDescription;
    public Sprite sprite;

    public TargetMode targetMode;

    public int skillCooldown;

    public EffectProperties[] gameEffects;
}

/// <summary>
/// An instance of a GameEffect to be attached to an instanced Character
/// </summary>
public class AppliedEffect
{
    public BaseCharacter target;
    public BaseGameEffect referenceEffect;
    public int remainingTurns;
    public EffectStrength strength;
    public float[] customValues;

    /// <summary>
    /// </summary>
    /// <param name="target"></param>
    /// <returns>Is effect still active?</returns>
    public bool Tick()
    {
        remainingTurns--;
        referenceEffect.Tick(target, strength, customValues);
        if (remainingTurns == 0)
        {
            referenceEffect.OnExpire(target, strength, customValues);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Unlike Tick, doesn't activate the effect when called
    /// </summary>
    /// <returns>Is effect still active?</returns>
    public bool TickSilent()
    {
        remainingTurns--;
        if (remainingTurns == 0)
        {
            referenceEffect.OnExpire(target, strength, customValues);
            return false;
        }
        return true;
    }

    public string GetEffectDescription()
    {
        return referenceEffect.GetEffectDescription(strength, customValues);
    }
}

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
    public bool CanUse { get { return cooldownTimer == 0; } }

    public SkillObject referenceSkill { get; private set; }

    public void InitWithSkill(SkillObject reference)
    {
        referenceSkill = reference;
    }

    public void BeginCooldown()
    {
        cooldownTimer = referenceSkill.skillCooldown;
    }

    public void Cooldown()
    {
        cooldownTimer = Mathf.Clamp(cooldownTimer - 1, 0, referenceSkill.skillCooldown);
    }
}