using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static Facade;

public class GameEffectLoader : BasicSingleton<GameEffectLoader>
{
    [SerializeField] List<BaseGameEffect> gameEffects = new();

    public int GetEffectIndex(BaseGameEffect e) => gameEffects.IndexOf(e);
    public BaseGameEffect FromIndex(int i) => gameEffects[i];

    public BattleState.SerializedEffect SerializeGameEffect(AppliedEffect effect)
    {
        BattleState.SerializedEffect se = new BattleState.SerializedEffect();

        var characters = new List<BaseCharacter>(battleSystem.PlayerCharacters);
        characters.AddRange(enemyController.EnemyList);
        se.Caster = characters.IndexOf(effect.Caster);

        se.EffectIndex = GetEffectIndex(effect.referenceEffect);
        if (se.EffectIndex == -1)
        {
            Debug.LogError(nameof(GameEffectLoader) + ": Serialization failed! Effect " +
                effect.referenceEffect + " not added to gameEffects list!");
        }
        se.RemainingTurns = effect.remainingTurns;
        se.StartingTurns = effect.startingTurns;
        se.CachedValues = effect.cachedValues;
        if (effect.value != null) se.Values = effect.value.Serialize();
        se.Stacks = effect.Stacks;

        return se;
    }

    public AppliedEffect DeserializeEffect(BattleState.SerializedEffect effect, BaseCharacter target)
    {
        AppliedEffect ae = new AppliedEffect(FromIndex(effect.EffectIndex));

        var characters = new List<BaseCharacter>(battleSystem.PlayerCharacters);
        characters.AddRange(enemyController.EnemyList);

        ae.targetProps = new TargetProps();
        ae.targetProps.Caster = characters[effect.Caster];
        ae.targetProps.Targets = new BaseCharacter[] { target };
        ae.remainingTurns = effect.RemainingTurns;
        ae.startingTurns = effect.StartingTurns;
        ae.cachedValues = effect.CachedValues;
        if (effect.Values != null) ae.value = BattleState.SerializedValue.Deserialize(effect.Values);
        ae.stacks = effect.Stacks;

        return ae;
    }
}