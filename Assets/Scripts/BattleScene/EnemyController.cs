using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class EnemyController : BasicSingleton<EnemyController>
{
    public EnemyCharacter[] Enemies { get; private set; }

    public EnemyCharacter RandomEnemy
    {
        get
        {
            List<EnemyCharacter> e = new List<EnemyCharacter>();
            for (int i = 0; i < Enemies.Length; i++)
            {
                if (Enemies[i])
                {
                    if (!Enemies[i].IsDead) e.Add(Enemies[i]);
                }
            }

            if (e.Count == 0) return null;
            battleStateManager.InitializeRandom();
            return e[Random.Range(0, e.Count)];
        }
    }

    public bool EnemiesAlive
    {
        get
        {
            for (int i = 0; i < Enemies.Length; i++)
            {
                if (Enemies[i])
                {
                    if (!Enemies[i].IsDead) return true;
                }
            }

            return false;
        }
    }

#if UNITY_EDITOR
    [Header("Debug Options")] [SerializeField]
    bool disableSkillUsage;
#endif

    bool useSkill;
    public bool WillUseSkill => useSkill;

    public static System.Action OnChangedAttackers;
    public static System.Action OnChangedAttackTargets;

    private void OnEnable()
    {
        BattleSystem.OnTargettableCharactersChanged += ChooseAttackTarget;
        GlobalEvents.OnCharacterFinishSuperCritical += OnCharacterFinishSuperCritical;
    }

    private void OnDisable()
    {
        BattleSystem.OnTargettableCharactersChanged -= ChooseAttackTarget;
        GlobalEvents.OnCharacterFinishSuperCritical -= OnCharacterFinishSuperCritical;
    }

    //void Update()
    //{
    //
    //}

    public void AssignEnemies(List<EnemyCharacter> enemyCharacters)
    {
        Enemies = enemyCharacters.ToArray();
    }

    public void ChooseNewTargets()
    {
        ChooseAttacker();
        ChooseAttackTarget();
    }

    public void CalculateSkillUsage()
    {
        battleStateManager.InitializeRandom();
        useSkill = Random.value <= battleSystem.ActiveEnemy.ChanceToUseSkill;
    }

    public void ChooseAttacker()
    {
        if (Enemies.Length == 0) return;
        battleSystem.SetEnemyAttacker(RandomEnemy);
        OnChangedAttackers?.Invoke();
    }

    public void ChooseAttackTarget()
    {
        battleSystem.SetEnemyAttackTarget(BattleSystem.Instance.RandomPlayerCharacter);
        OnChangedAttackTargets?.Invoke();
    }

    public void MakeYourMove()
    {
#if UNITY_EDITOR
        if (disableSkillUsage)
        {
            BeginAttack();
            return;
        }
#endif
        battleStateManager.InitializeRandom();

#if UNITY_EDITOR
        // Enemies should not have 0 skills for real, at least I think they shouldn't...
        if (battleSystem.ActiveEnemy.Skills.Count == 0)
        {
            BeginAttack();
            Debug.LogWarning(nameof(EnemyController) + 
                ": " + battleSystem.ActiveEnemy.Reference.characterName + 
                " tried to use a skill, but no skills found!");
            return;
        }
#endif

        if (useSkill)
        {
            GameSkill randomSkill =
                battleSystem.ActiveEnemy.Skills[Random.Range(0, battleSystem.ActiveEnemy.Skills.Count)];
            battleSystem.ActivateEnemySkill(battleSystem.ActiveEnemy, randomSkill);
        }
        else
        {
            BeginAttack();
        }
    }

    public void BeginAttack()
    {
        battleSystem.ActiveEnemy.BeginAttack(battleSystem.ActivePlayer.transform);
        if (battleSystem.ActiveEnemy.CanCrit)
        {
            if (battleSystem.ActiveEnemy.Reference.isSuperCriticalAnAttack)
            {
                ui.StartDefending();
            }
        }
        else
        {
            ui.StartDefending();
        }
    }

    public void RegisterEnemyDeath(EnemyCharacter enemy)
    {
        ChooseAttacker();
    }

    private void OnCharacterFinishSuperCritical(BaseCharacter obj)
    {
        var enemy = obj as EnemyCharacter;
        if (enemy)
        {
            enemy.ResetChargeLevel();
            enemyController.MakeYourMove();
        }
    }

    #region Debug Hacks

    [IngameDebugConsole.ConsoleMethod(nameof(CrippleEnemies), "Instantly hurt enemies, leaving them at 1 health")]
    public static void CrippleEnemies()
    {
        for (int i = 0; i < Instance.Enemies.Length; i++)
        {
            if (!Instance.Enemies[i]) continue;
            BaseCharacter.IncomingDamage.damage = Instance.Enemies[i].CurrentHealth - 1;
            Instance.Enemies[i].TakeDamage();
        }

        Debug.Log("Enemies damaged!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(TestEnemySupers), "Macro that executes MaxEnemyCrit and ForceEnemyNoSkills")]
    public static void TestEnemySupers()
    {
        MaxEnemyCrit();
        ForceEnemyNoSkills();
    }

    [IngameDebugConsole.ConsoleMethod(nameof(MaxEnemyCrit), "Set enemy character's crit chance to 100%")]
    public static void MaxEnemyCrit()
    {
        for (int i = 0; i < Instance.Enemies.Length; i++)
        {
            if (!Instance.Enemies[i]) continue;
            var diff = Instance.Enemies[i].Reference.turnsToCrit - Instance.Enemies[i].CritLevel;
            Instance.Enemies[i].IncreaseChargeLevel(diff);
        }

        Debug.Log("Enemy crit maxed!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(ForceEnemyUseSkills), "Sets enemy character's skill use chance to 100%")]
    public static void ForceEnemyUseSkills()
    {
        for (int i = 0; i < Instance.Enemies.Length; i++)
        {
            if (!Instance.Enemies[i]) continue;
            Instance.Enemies[i].SetSkillUseChance(1);
        }

        Debug.Log("Set enemy skill chance to 100%!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(ForceEnemyNoSkills), "Sets enemy character's skill use chance to 0%")]
    public static void ForceEnemyNoSkills()
    {
        for (int i = 0; i < Instance.Enemies.Length; i++)
        {
            if (!Instance.Enemies[i]) continue;
            Instance.Enemies[i].SetSkillUseChance(0);
            Instance.useSkill = false;
#if UNITY_EDITOR
            Instance.disableSkillUsage = true;
#endif
        }

        Debug.Log("Enemies skill use disabled!");
    }

    #endregion
}