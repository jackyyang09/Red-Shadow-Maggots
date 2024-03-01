using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

[CreateAssetMenu(fileName = "Damage Effect", menuName = "ScriptableObjects/Game Effects/Instant Damage", order = 1)]
public class InstantDamageEffect : BaseDamageEffect
{
    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count == 0)
        {
            effect.cachedValues.Add(GetValue(stat, effect.values[0],effect.target));
        }

        DamageStruct damageStruct = new DamageStruct
        {
            Percentage = 1,
            TrueDamage = effect.cachedValues[0],
            Effectivity = DamageEffectivess.Normal,
            QTEResult = QuickTimeBase.QTEResult.Perfect
        };
        effect.target.TakeDamage(damageStruct);

        switch (battleSystem.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                damageStruct.QTEPlayer = (!effect.target.IsPlayer()).ToInt();
                damageStruct.QTEEnemy = 1 - damageStruct.QTEPlayer;
                break;
            case BattlePhases.EnemyTurn:
                damageStruct.QTEEnemy = effect.target.IsPlayer().ToInt();
                damageStruct.QTEPlayer = 1 - damageStruct.QTEEnemy;
                break;
        }

        return base.Activate(effect);
    }
}