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
            effect.cachedValues.Add(GetValue(stat, effect.values[0],effect.Target));
        }

        DamageStruct damageStruct = new DamageStruct
        {
            Percentage = 1,
            TrueDamage = effect.cachedValues[0],
            Effectivity = DamageEffectivess.Normal,
            QTEResult = QuickTimeBase.QTEResult.Perfect
        };
        effect.Target.TakeDamage(damageStruct);

        switch (battleSystem.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                damageStruct.QTEPlayer = (!effect.Target.IsPlayer()).ToInt();
                damageStruct.QTEEnemy = 1 - damageStruct.QTEPlayer;
                break;
            case BattlePhases.EnemyTurn:
                damageStruct.QTEEnemy = effect.Target.IsPlayer().ToInt();
                damageStruct.QTEPlayer = 1 - damageStruct.QTEEnemy;
                break;
        }

        return base.Activate(effect);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.Self:
                s += "Take ";
                break;
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "take ";
                break;
        }

        s += RSMConstants.Keywords.Short.DAMAGE + " equal to " +
            EffectValueDescriptor(props.effectValues[0], "your", stat);

        return s;
    }
}