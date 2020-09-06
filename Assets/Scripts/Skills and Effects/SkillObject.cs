using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetMode
{
    None,
    Allies,
    Enemies,
    AlliesAndEnemies
}

[CreateAssetMenu(fileName = "New Skill", menuName = "ScriptableObjects/Skill Object", order = 1)]
public class SkillObject : ScriptableObject
{
    public string skillName;
    public string skillDescription;
    public Sprite sprite;

    public TargetMode targetMode;

    public BaseGameEffect[] effects;
}

public class GameSkill
{
    /// <summary>
    /// How long to wait before skill recharges
    /// </summary>
    int cooldownTimer;

    /// <summary>
    /// Destroys itself when it runs out of these
    /// </summary>
    List<BaseGameEffect> effects;

    SkillObject referenceSkill;

    public void InitWithSkill(SkillObject reference)
    {
        referenceSkill = reference;
        cooldownTimer = 0;
        //reference.effects
    }

    public void Activate()
    {

    }

    public void Tick()
    {

    }
}