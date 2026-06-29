using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Leader was going to have a passive ability that applies to all Allies, 
/// but having effects that originate from an AppliedEffect attached to a different character is difficult 
/// to implement from a code and UI perspective. It's better to have one source effect give each character 
/// an AppliedEffect. 
/// The way the system is implemented currently, all AppliedEffects get serialized and saved to be loaded when the
/// game starts up. I'm too stubborn to give up on save/loading, which puts me at a standstill, forced to redesign
/// the effect rather than rewrite another system for the umpteenth time
/// </summary>
public class SniperGangLeaderPassive : BaseCharacterPassive
{
    [SerializeField] BaseGameEffect sniperGruntStack;
    [SerializeField] BaseGameEffect sniperGangRevive;
    [SerializeReference, SubclassSelector] BaseGameStat attack;
    [SerializeField] float atkUp = 0.05f;
    [SerializeReference, SubclassSelector] BaseGameStat wait;
    [SerializeField] float waitDown = 0.04f;
    int activeStacks;

    protected override void Init()
    {
        baseCharacter.OnDealDamage += OnDealDamage;
        BaseCharacter.OnAppliedEffect += OnAppliedEffect;
        BaseCharacter.OnRemoveEffect += OnRemovedEffect;
        BaseCharacter.OnCharacterDeath += OnCharacterDeath;

        BattleSystem.OnInitialized += Initialized;
    }

    protected override void Cleanup()
    {
        baseCharacter.OnDealDamage -= OnDealDamage;
        BaseCharacter.OnAppliedEffect -= OnAppliedEffect;
        BaseCharacter.OnRemoveEffect -= OnRemovedEffect;
        BaseCharacter.OnCharacterDeath -= OnCharacterDeath;
    }

    void Initialized()
    {
        BattleSystem.OnInitialized -= Initialized;

        foreach (var e in EnemyController.Instance.LivingEnemies)
        {
            if (!e.EffectDictionary.TryGetValue(sniperGruntStack, out effectList)) continue;

            activeStacks += effectList.Count;
        }

        if (activeStacks > 0) UpdateBuff(0);
    }

    void OnDealDamage(BaseCharacter target)
    {
        int stackCount = 0;
        AppliedEffect spareParts = null;
        if (baseCharacter.EffectDictionary.ContainsKey(sniperGangRevive))
        {
            spareParts = baseCharacter.EffectDictionary[sniperGangRevive][0];
            stackCount = spareParts.Stacks;
        }

        // I wanted to use maxStacks, but maxStacks REALLY should not be defined in the EffectGroup struct
        if (target.EffectDictionary.ContainsKey(sniperGruntStack) && stackCount < 4)
        {
            var e = target.EffectDictionary[sniperGruntStack][0];
            if (e.Stacks > 0)
            {
                e.Stacks--;
                spareParts.Stacks++;
            }
        }
    }

    void UpdateBuff(int prev)
    {
        if (prev > 0 && prev != activeStacks)
        {
            attack.SetGameStat(baseCharacter, attack.GetGameStat(baseCharacter) * -atkUp * prev);
            wait.SetGameStat(baseCharacter, wait.GetGameStat(baseCharacter) * -waitDown * prev);
        }

        attack.SetGameStat(baseCharacter, attack.GetGameStat(baseCharacter) * atkUp * activeStacks);
        wait.SetGameStat(baseCharacter, wait.GetGameStat(baseCharacter) * waitDown * activeStacks);
    }

    void OnAppliedEffect(BaseCharacter character, AppliedEffect effect)
    {
        // For the sake of finishing this project one day
        // Increase ATK and decrease WAIT of SELF rather than team members
        if (effect.referenceEffect != sniperGruntStack) return;

        int prev = activeStacks;
        activeStacks += effect.Stacks;

        UpdateBuff(prev);
    }

    void OnRemovedEffect(BaseCharacter character, AppliedEffect effect)
    {
        if (effect.referenceEffect != sniperGruntStack) return;

        int prev = activeStacks;
        activeStacks += effect.Stacks;

        UpdateBuff(prev);
    }

    List<AppliedEffect> effectList;
    void OnCharacterDeath(BaseCharacter character)
    {
        if (!character.EffectDictionary.TryGetValue(sniperGruntStack, out effectList)) return;

        int prev = activeStacks;
        activeStacks -= effectList.Count;

        UpdateBuff(prev);
    }
}