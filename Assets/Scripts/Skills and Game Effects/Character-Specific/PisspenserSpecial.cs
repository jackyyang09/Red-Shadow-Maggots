using DocumentFormat.OpenXml.Office.CustomXsn;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "Pisspenser Upgrade", menuName = "ScriptableObjects/Character-Specific/Piss/Pisspenser Upgrade", order = 1)]
public class PisspenserSpecial : BaseGameEffect, IStackableEffect
{
    [SerializeField] float attackIncrease = 0.01f;
    [SerializeField] float defenseIncrease = 0.01f;
    [SerializeField] float healMultiplier = 0.01f;
    [SerializeField] float shieldMultiplier = 0.01f;

    delegate List<BaseCharacter> AllyDelegate();
    AllyDelegate getAllies;

    public override bool Activate(AppliedEffect effect)
    {
        effect.target.OnStartTurn += OnStartTurn;

        if (effect.target.IsPlayer())
        {
            getAllies = () => BattleSystem.Instance.LivingPlayers.ToList<BaseCharacter>();
        }
        else
        {
            getAllies = () => EnemyController.Instance.LivingEnemies.ToList<BaseCharacter>();
        }

        return base.Activate(effect);
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        target.OnStartTurn += OnStartTurn;

        base.OnExpire(user, target, strength, customValues);
    }

    void OnStartTurn()
    {
        var allies = getAllies();

        foreach (var ally in allies)
        {
            ally.Heal(ally.MaxHealth * healMultiplier);
            Debug.LogWarning(nameof(PisspenserSpecial) +
                ": Shields should be doled out through an application of a basic effect");
            ally.GiveShield(ally.MaxHealth * shieldMultiplier, null);
        }
    }

    public void OnStacksChanged(AppliedEffect effect)
    {
        var allies = getAllies();

        foreach (var ally in allies)
        {
            if (effect.cachedValues.Count > 0)
            {
                ally.ApplyAttackModifier(effect.cachedValues[0]);
                ally.ApplyDefenseModifier(effect.cachedValues[1]);
            }

            var amount = ally.Attack * attackIncrease * effect.Stacks;
            ally.ApplyAttackModifier(amount);
            effect.cachedValues.Add(amount);

            amount = defenseIncrease * effect.Stacks;
            ally.ApplyDefenseModifier(amount);
            effect.cachedValues.Add(amount);
        }
    }

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        string d = "Each stack " +
            "increases " + RSMConstants.Keywords.Short.ATTACK + " by " + attackIncrease.FormatPercentage() +
            " and " + RSMConstants.Keywords.Short.DEFENSE + " by " + defenseIncrease.FormatPercentage() +
            " for " + TargetModeDescriptor(TargetMode.AllAllies).TrimEnd() + ". " +
            "Every turn, " + TargetModeDescriptor(TargetMode.AllAllies) + "recover " + 
            RSMConstants.Keywords.Short.HEALTH + " equal to " + healMultiplier.FormatPercentage() + " of their " +
            RSMConstants.Keywords.Short.MAX_HEALTH + " and receive a Shield with a strength of " + 
            shieldMultiplier.FormatPercentage() + " of their " + RSMConstants.Keywords.Short.MAX_HEALTH + ".";

        return d;
    }
}