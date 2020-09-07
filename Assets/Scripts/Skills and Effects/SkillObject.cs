using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetMode
{
    None,
    OneAlly,
    OneEnemy,
    AllAllies,
    AllEnemies
}

[CreateAssetMenu(fileName = "New Skill", menuName = "ScriptableObjects/Skill Object", order = 1)]
public class SkillObject : ScriptableObject
{
    public string skillName;
    public string skillDescription;
    public Sprite sprite;

    public TargetMode targetMode;

    public BaseGameEffect[] effects;

    public int skillCooldown;
}


public class AppliedEffect
{
    public BaseGameEffect effect;
    public int remainingTurns;
}

public class GameSkill
{
    /// <summary>
    /// How long to wait before skill recharges
    /// </summary>
    public int cooldownTimer;

    /// <summary>
    /// Destroys itself when it runs out of these
    /// </summary>
    public List<AppliedEffect> effects = new List<AppliedEffect>();

    public SkillObject referenceSkill { get; private set; }

    public void InitWithSkill(SkillObject reference)
    {
        referenceSkill = reference;
    }

    public void Activate(List<BaseCharacter> characters)
    {
        cooldownTimer = referenceSkill.skillCooldown;
        foreach (BaseGameEffect effect in referenceSkill.effects)
        {
            AppliedEffect newEffect = new AppliedEffect();
            newEffect.effect = effect;
            newEffect.remainingTurns = effect.effectDuration;
            effect.Activate(characters);
            effects.Add(newEffect);
        }
    }

    public void Tick()
    {
        foreach (AppliedEffect effect in effects)
        {
            effect.remainingTurns--;
        }
    }
}