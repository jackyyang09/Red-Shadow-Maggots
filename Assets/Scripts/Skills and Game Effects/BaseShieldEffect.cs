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
        "Takes " + RSMConstants.Keywords.Short.DAMAGE + " in place of " + RSMConstants.Keywords.Short.HEALTH + ". ";
        //RSMConstants.Keywords.Short.DAMAGE + " taken is reduced by " + RSMConstants.Keywords.Short.DEFENSE + ".";

    BaseCharacter targetCharacter;
    ForceFieldFX forceFieldInstance;

    public override bool Activate(AppliedEffect effect)
    {
        float percentageChange = value.GetStrength(effect.strength);
        var target = effect.target;

        switch (stat)
        {
            case ScalingStat.Health:
                target.GiveShield(effect.caster.MaxHealth * percentageChange, effect);
                break;
            case ScalingStat.Attack:
                target.GiveShield(effect.caster.AttackModified * percentageChange, effect);
                break;
        }

        targetCharacter = target;

        if (effect.target.ShieldPercent == 0)
        {
            forceFieldInstance = Instantiate(forceFieldPrefab, target.CharacterMesh.transform).GetComponent<ForceFieldFX>();
            forceFieldInstance.Initialize(target);

            target.OnShieldBroken += OnShieldBroken;
        }

        return true;
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        base.OnExpire(user, target, strength, customValues);
        Destroy(forceFieldInstance.gameObject);
    }

    void OnShieldBroken()
    {
        targetCharacter.OnShieldBroken -= OnShieldBroken;
        Destroy(forceFieldInstance.gameObject);
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        return ExplainerDescription;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        float percentageChange = value.GetStrength(props.strength);

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
}