using System.Linq;
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
            var e = Enemies.Where(item => item != null).ToList();
            var notDead = e.Where(item => !item.IsDead).ToList();
            if (notDead.Count == 0) return null;
            battleStateManager.InitializeRandom();
            return notDead[Random.Range(0, notDead.Count)];
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

    Dictionary<EnemyCharacter, bool> useSkill = new Dictionary<EnemyCharacter, bool>();
    public bool WillUseSkill => false; // Battle Line may be deprecated

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

    public void AssignEnemies(EnemyCharacter[] enemyCharacters)
    {
        Enemies = enemyCharacters;
    }

    public void CalculateSkillUsage()
    {
        battleStateManager.InitializeRandom();
        useSkill.Clear();
        for (int i = 0; i < Enemies.Length; i++)
        {
            if (Enemies[i])
            {
                useSkill.Add(Enemies[i], Random.value <= Enemies[i].ChanceToUseSkill);
            }
        }
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

        if (useSkill[battleSystem.ActiveEnemy] && !battleSystem.ActiveEnemy.UsedSkillThisTurn)
        {
            GameSkill randomSkill =
                battleSystem.ActiveEnemy.Skills[Random.Range(0, battleSystem.ActiveEnemy.Skills.Count)];
            ActivateEnemySkill(battleSystem.ActiveEnemy, randomSkill);
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

    public void ActivateEnemySkill(EnemyCharacter enemy, GameSkill skill)
    {
        StartCoroutine(EnemySkillSequence(enemy, skill));
    }

    IEnumerator EnemySkillSequence(EnemyCharacter enemy, GameSkill skill)
    {
        bool finished = false;
        float skillUseTime = 1.5f;

        // Activate Skill
        enemy.UseSkill(skill);

        enemy.AnimHelper.RegisterOnFinishSkillAnimation(() => finished = true);

        SceneTweener.Instance.SkillTween(enemy.transform, skillUseTime);

        //yield return new WaitForSeconds(skillUseTime);
        while (!finished) yield return null;

        SceneTweener.Instance.SkillUntween();

        finished = false;

        enemy.RegisterOnFinishApplyingSkillEffects(() => finished = true);

        enemy.ResolveSkill();

        // Wait for skill effects to finish animating
        while (!finished) yield return null;

        battleSystem.EndTurn();
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
            if (Instance.Enemies[i].IsDead) continue;
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
        Instance.useSkill.Clear();
        foreach (var item in Instance.Enemies)
        {
            if (!item) continue;
            item.SetSkillUseChance(0);
            Instance.useSkill[item] = false;
        }

#if UNITY_EDITOR
        Instance.disableSkillUsage = true;
#endif

        Debug.Log("Enemies skill use disabled!");
    }

    #endregion
}