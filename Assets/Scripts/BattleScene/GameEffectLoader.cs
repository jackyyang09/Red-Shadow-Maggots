using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static Facade;

public class GameEffectLoader : BasicSingleton<GameEffectLoader>
{
    [SerializeField] List<BaseGameEffect> gameEffects = new List<BaseGameEffect>();

    public int GetEffectIndex(BaseGameEffect e) => gameEffects.IndexOf(e);

    public BattleState.SerializedEffect SerializeGameEffect(AppliedEffect effect)
    {
        BattleState.SerializedEffect se = new BattleState.SerializedEffect();

        var characters = new List<BaseCharacter>(battleSystem.PlayerCharacters);
        characters.AddRange(enemyController.EnemyList);
        se.Caster = characters.IndexOf(effect.caster);

        se.EffectIndex = GetEffectIndex(effect.referenceEffect);
        if (se.EffectIndex == -1)
        {
            Debug.LogError(nameof(GameEffectLoader) + ": Serialization failed! Effect " +
                effect.referenceEffect + " not added to gameEffects list!");
        }
        se.RemainingTurns = effect.remainingTurns;
        se.StartingTurns = effect.startingTurns;
        se.CachedValues = effect.cachedValues;
        se.Values = effect.values;
        return se;
    }

    public AppliedEffect DeserializeEffect(BattleState.SerializedEffect effect, BaseCharacter target)
    {
        AppliedEffect ae = new AppliedEffect(gameEffects[effect.EffectIndex]);

        var characters = new List<BaseCharacter>(battleSystem.PlayerCharacters);
        characters.AddRange(enemyController.EnemyList);

        ae.caster = characters[effect.Caster];
        ae.target = target;
        ae.remainingTurns = effect.RemainingTurns;
        ae.startingTurns = effect.StartingTurns;
        ae.cachedValues = effect.CachedValues;
        ae.values = effect.Values;

        return ae;
    }
}