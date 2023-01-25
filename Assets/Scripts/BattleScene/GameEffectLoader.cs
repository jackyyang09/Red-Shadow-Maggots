using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEffectLoader : BasicSingleton<GameEffectLoader>
{
    [SerializeField] List<BaseGameEffect> gameEffects = new List<BaseGameEffect>();

    public BattleState.SerializedEffect SerializeGameEffect(AppliedEffect effect)
    {
        BattleState.SerializedEffect se = new BattleState.SerializedEffect();
        se.EffectIndex = gameEffects.IndexOf(effect.referenceEffect);
        se.RemainingTurns = effect.remainingTurns;
        se.Strength = effect.strength;
        se.CustomValues = effect.customValues;
        return se;
    }

    public AppliedEffect DeserializeEffect(BattleState.SerializedEffect effect, BaseCharacter target)
    {
        AppliedEffect ae = new AppliedEffect();
        ae.target = target;
        ae.referenceEffect = gameEffects[effect.EffectIndex];
        ae.remainingTurns = effect.RemainingTurns;
        ae.strength = effect.Strength;
        ae.customValues = effect.CustomValues;
        ae.description = ae.referenceEffect.GetEffectDescription(TargetMode.Self, ae.strength, ae.customValues, 0);
        return ae;
    }
}