using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sniper Gang Revive", 
    menuName = "ScriptableObjects/Character-Specific/Sniper Gang Revive", order = 1)]
public class SniperGangRevive : BaseGameEffect, IStackableEffect
{
    [SerializeField] float waitAmount = 0.05f;
    [SerializeReference, SubclassSelector] BaseGameStat waitStat;
    [SerializeReference, SubclassSelector] BaseEffectValue healPerStack;

    public override string ExplainerName => "Spare Parts";
    public override string ExplainerDescription =>
        "Each stack increases WAIT LIMIT by 5 (Max 4). " +
        "The following effect triggers on death or at the end of battle: " +
        "With 2+ stacks, consume all stacks to revive with 1 HP, then for each stack consumed, " +
        "heal self equal to 10% of MAX HP.";

    public override bool Activate(AppliedEffect effect)
    {
        OnStacksChanged(effect, 0);
        return base.Activate(effect);
    }

    public void OnStacksChanged(AppliedEffect effect, int previous)
    {
        waitStat.SetGameStat(effect.Target, -waitAmount * previous);
        waitStat.SetGameStat(effect.Target, waitAmount * effect.Stacks);
    }

    public override void OnDeath(AppliedEffect effect)
    {
        Debug.Log("ON DEATH");
        effect.Target.Heal(healPerStack.GetValue(effect.targetProps) * effect.Stacks);
        effect.Remove();
    }
}