using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class GameEffectLoader : BasicSingleton<GameEffectLoader>
{
    [SerializeField] List<BaseGameEffect> gameEffects = new List<BaseGameEffect>();

    public BattleState.SerializedEffect SerializeGameEffect(AppliedEffect effect)
    {
        BattleState.SerializedEffect se = new BattleState.SerializedEffect();

        var characters = new List<BaseCharacter>(battleSystem.PlayerCharacters);
        characters.AddRange(enemyController.EnemyList);
        se.Caster = characters.IndexOf(effect.caster);

        se.EffectIndex = gameEffects.IndexOf(effect.referenceEffect);
        if (se.EffectIndex == -1)
        {
            Debug.LogError(nameof(GameEffectLoader) + ": Serialization failed! Effect " +
                effect.referenceEffect + " not added to gameEffects list!");
        }
        se.RemainingTurns = effect.remainingTurns;
        se.Strength = effect.strength;
        se.CustomValues = effect.customValues;
        return se;
    }

    public AppliedEffect DeserializeEffect(BattleState.SerializedEffect effect, BaseCharacter target)
    {
        AppliedEffect ae = new AppliedEffect();

        var characters = new List<BaseCharacter>(battleSystem.PlayerCharacters);
        characters.AddRange(enemyController.EnemyList);
        ae.caster = characters[effect.Caster];

        ae.target = target;
        ae.referenceEffect = gameEffects[effect.EffectIndex];
        ae.remainingTurns = effect.RemainingTurns;
        ae.strength = effect.Strength;
        ae.customValues = effect.CustomValues;
        ae.description = ae.referenceEffect.GetEffectDescription(TargetMode.Self, ae.strength, ae.customValues, 0);
        return ae;
    }
}