using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Facade;

namespace SuperCrits
{
    public class BaseSuperCrit : BaseCharacterPassive
    {
        SuperCritObject SuperCrit => baseCharacter.Reference.superCritical;

        protected override void Init()
        {
            baseCharacter.OnBeginSuperCrit += OnBeginSuperCrit;
        }

        protected override void Cleanup()
        {
            baseCharacter.OnBeginSuperCrit -= OnBeginSuperCrit;
        }

        //public void DealSCDamage()
        //{
        //    BaseCharacter.IncomingDamage.Percentage = damageMultiplier;
        //    if (baseCharacter.IsPlayer())
        //    {
        //        BaseCharacter.IncomingDamage.QTEPlayer = 1;
        //        BaseCharacter.IncomingDamage.QTEEnemy = 0;
        //    }
        //    else
        //    {
        //        BaseCharacter.IncomingDamage.QTEPlayer = 0;
        //        BaseCharacter.IncomingDamage.QTEEnemy = 1;
        //    }
        //
        //    BaseCharacter.OnCharacterExecuteAttack?.Invoke(baseCharacter);
        //
        //    baseCharacter.DealDamage(battleSystem.OpposingCharacter);
        //}

        //public void DealAOESCDamage()
        //{
        //    BaseCharacter.IncomingDamage.Percentage = damageMultiplier;
        //    BaseCharacter.IncomingDamage.IsAOE = true;
        //
        //    var targets = new List<BaseCharacter>();
        //    if (battleSystem.CurrentPhase == BattlePhases.PlayerTurn)
        //    {
        //        targets.AddRange(enemyController.LivingEnemies);
        //    }
        //    else if (battleSystem.CurrentPhase == BattlePhases.EnemyTurn)
        //    {
        //        targets.AddRange(battleSystem.LivingPlayers);
        //    }
        //
        //    BaseCharacter.OnCharacterExecuteAttack?.Invoke(baseCharacter);
        //
        //    foreach (var character in targets)
        //    {
        //        baseCharacter.DealDamage(character);
        //    }
        //}

        protected List<BaseCharacter> GetEffectTargets(TargetMode mode)
        {
            switch (mode)
            {
                case TargetMode.None:
                case TargetMode.OneAlly:
                    Debug.LogError("Cannot get target from " + mode);
                    break;
                case TargetMode.OneEnemy:
                    return new List<BaseCharacter> { battleSystem.OpposingCharacter };
                case TargetMode.AllAllies:
                    switch (battleSystem.CurrentPhase)
                    {
                        case BattlePhases.PlayerTurn:
                            return enemyController.EnemyList.ToList<BaseCharacter>();
                        case BattlePhases.EnemyTurn:
                            return battleSystem.PlayerList.ToList<BaseCharacter>();
                    }
                    break;
                case TargetMode.AllEnemies:
                    switch (battleSystem.CurrentPhase)
                    {
                        case BattlePhases.PlayerTurn:
                            return enemyController.EnemyList.ToList<BaseCharacter>();
                        case BattlePhases.EnemyTurn:
                            return battleSystem.PlayerList.ToList<BaseCharacter>();
                    }
                    break;
                case TargetMode.Self:
                    return new List<BaseCharacter> { baseCharacter };
            }
            return null;
        }

        void OnBeginSuperCrit()
        {
            effectsApplied = 0;
        }

        int effectsApplied = 0;
        public void ApplyNextSuperCritEffect()
        {
            var effectEvent = SuperCrit.events[effectsApplied];
            var group = SuperCrit.effects[effectEvent.effectIndex].Copy();
            var targets = GetEffectTargets(group.targetOverride);

            foreach (var v in group.damageProps.effectValues)
            {
                v.multiplier *= effectEvent.damageModifier;
                v.flat *= effectEvent.damageModifier;
            }

            TargetProps targetProps = new()
            {
                Caster = baseCharacter,
                Targets = targets.ToArray(),
                TargetMode = group.targetOverride
            };

            StartCoroutine(group.appStyle.Apply(group, targetProps));

            effectsApplied++;
        }

        public void ApplyNextSuperCritDMGEffect()
        {
            //var targets = GetEffectTargets(SuperCrit.damageEffects[damageApplied].targetOverride);
            //foreach (var t in targets)
            //{
            //    BaseCharacter.ApplyEffectToCharacter(SuperCrit.damageEffects[damageApplied], baseCharacter, t);
            //}
            //damageApplied++;
        }
    }
}