using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield Effect", menuName = "ScriptableObjects/Game Effects/Shield", order = 1)]
public class BaseShieldEffect : BaseGameEffect
{
    public enum ScalingStat
    {
        Health,
        Attack
    }

    [SerializeField] ScalingStat stat;
    [SerializeField] GameObject forceFieldPrefab;

    public GameStatValue value;

    /// <summary>
    /// Applies a Shield to all allies, absorbing DMG equal to 45% of Gepard's DEF plus 600 for 3 turn(s).
    /// </summary>
    public override bool IncludesExplainer => true;
    public override string ExplainerName => "Shield";
    public override string ExplainerDescription =>
        "Takes " + RSMConstants.Keywords.Short.DAMAGE + " in place of " + RSMConstants.Keywords.Short.HEALTH + ". " +
        RSMConstants.Keywords.Short.DAMAGE + " taken is reduced by " + RSMConstants.Keywords.Short.DEFENSE + ".";

    BaseCharacter targetCharacter;
    ForceFieldFX forceFieldInstance;

    public override bool Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        switch (stat)
        {
            case ScalingStat.Health:
                target.GiveShield(user.MaxHealth * percentageChange);
                break;
            case ScalingStat.Attack:
                target.GiveShield(user.AttackModified * percentageChange);
                break;
        }

        targetCharacter = target;

        forceFieldInstance = Instantiate(forceFieldPrefab, target.CharacterMesh.transform).GetComponent<ForceFieldFX>();
        forceFieldInstance.Initialize(target);

        target.OnShieldBroken += OnShieldBroken;

        return true;
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        base.OnExpire(user, target, strength, customValues);
        Destroy(forceFieldInstance.gameObject);
    }

    void OnShieldBroken()
    {
        targetCharacter.RemoveAllEffectsOfType(this, true);
        Destroy(forceFieldInstance.gameObject);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        float percentageChange = (float)GetEffectStrength(props.strength, props.customValues);

        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.None:
            case TargetMode.Self:
                s = "Receive ";
                break;
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += "receives ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "receive ";
                break;
        }

        /// <summary>
        /// Applies a Shield to all allies, absorbing DMG equal to 45% of Gepard's DEF plus 600 for 3 turn(s).
        /// </summary>

        s += percentageChange * 100 + "% of your ";

        switch (stat)
        {
            case ScalingStat.Health:
                s += RSMConstants.Keywords.Short.MAX_HEALTH;
                break;
            case ScalingStat.Attack:
                s += RSMConstants.Keywords.Short.ATTACK;
                break;
        }

        s += " as a <u>Shield</u>";

        return s + " " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return customValues[0];
            case EffectStrength.Weak:
                return 0.5f;
            case EffectStrength.Small:
                return 0.75f;
            case EffectStrength.Medium:
                return 1f;
            case EffectStrength.Large:
                return 1.5f;
            case EffectStrength.EX:
                return 2f;
        }
        return 0;
    }
}
