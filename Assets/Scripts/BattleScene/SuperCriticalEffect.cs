using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Facade;

/// <summary>
/// Deprecated
/// </summary>
public abstract class SuperCriticalEffect : MonoBehaviour
{
    [SerializeField] protected AnimationHelper animHelper = null;
    protected BaseCharacter baseCharacter { get { return animHelper.BaseCharacter; } }

    public virtual void DealSuperCritDamage()
    {

    }

    public virtual void BeginSuperCritEffect()
    {

    }

    public virtual void FinishSuperCritEffect()
    {
        GlobalEvents.OnCharacterFinishSuperCritical?.Invoke(baseCharacter);
        effectsApplied = 0;
    }

    int effectsApplied = 0;

    public void ApplyNextSuperCritEffect()
    {
        var superCrit = baseCharacter.Reference.superCritical;
        if (effectsApplied >= superCrit.gameEffects.Length)
        {
            Debug.LogWarning(nameof(SuperCriticalEffect) + " - " + gameObject.name + ": " +
                "Tried to apply next super critical effect, but all effects have been applied!");
            return;
        }

        var effect = superCrit.gameEffects[effectsApplied];
        List<BaseCharacter> targets = new List<BaseCharacter>();
        var targetMode = effect.targetOverride == TargetMode.None ? superCrit.targetMode : effect.targetOverride;
        switch (targetMode)
        {
            case TargetMode.None:
            case TargetMode.Self:
                targets.Add(baseCharacter);
                break;
            case TargetMode.OneAlly:
                switch (battleSystem.CurrentPhase)
                {
                    case BattlePhases.PlayerTurn:
                        break;
                    case BattlePhases.EnemyTurn:
                        targets.Add(enemyController.RandomEnemy);
                        break;
                }
                //target = baseCharacter;
                break;
            case TargetMode.OneEnemy:
                switch (battleSystem.CurrentPhase)
                {
                    case BattlePhases.PlayerTurn:
                        targets.Add(battleSystem.ActiveEnemy);
                        break;
                    case BattlePhases.EnemyTurn:
                        targets.Add(battleSystem.ActivePlayer);
                        break;
                }
                break;
            case TargetMode.AllAllies:
                switch (battleSystem.CurrentPhase)
                {
                    case BattlePhases.PlayerTurn:
                        targets.AddRange(battleSystem.PlayerCharacters);
                        break;
                    case BattlePhases.EnemyTurn:
                        targets.AddRange(enemyController.Enemies);
                        break;
                }
                break;
            case TargetMode.AllEnemies:
                switch (battleSystem.CurrentPhase)
                {
                    case BattlePhases.PlayerTurn:
                        targets.AddRange(enemyController.Enemies);
                        break;
                    case BattlePhases.EnemyTurn:
                        targets.AddRange(battleSystem.PlayerCharacters);
                        break;
                }
                break;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            if (!targets[i]) continue;
            BaseCharacter.ApplyEffectToCharacter(effect, baseCharacter, targets[i]);
        }

        GlobalEvents.OnGameEffectApplied?.Invoke(superCrit.gameEffects[effectsApplied].effect);
        effectsApplied++;
    }
}