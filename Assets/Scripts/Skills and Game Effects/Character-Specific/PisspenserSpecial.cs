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

    public override bool Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        target.OnStartTurn += OnStartTurn;

        if (target.IsPlayer())
        {
            getAllies = () => BattleSystem.Instance.LivingPlayers.ToList<BaseCharacter>();
        }
        else
        {
            getAllies = () => EnemyController.Instance.LivingEnemies.ToList<BaseCharacter>();
        }

        return base.Activate(user, target, strength, customValues);
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
            ally.GiveShield(ally.MaxHealth * shieldMultiplier);
        }
    }

    public void OnStacksChanged(AppliedEffect effect)
    {
        var allies = getAllies();

        foreach (var ally in allies)
        {
            if (effect.cachedValue.Count > 0)
            {
                ally.ApplyAttackModifier(effect.cachedValue[0]);
                ally.ApplyDefenseModifier(effect.cachedValue[1]);
            }

            var amount = attackIncrease * effect.Stacks;
            ally.ApplyAttackModifier(amount);
            effect.cachedValue.Add(amount);

            amount = defenseIncrease * effect.Stacks;
            ally.ApplyDefenseModifier(amount);
            effect.cachedValue.Add(amount);
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