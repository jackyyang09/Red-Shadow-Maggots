﻿using System.Collections;
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

    public static System.Action OnChangedAttackers;
    public static System.Action OnChangedAttackTargets;

    private void OnEnable()
    {
        BattleSystem.OnTargettableCharactersChanged += ChooseAttackTarget;
    }

    private void OnDisable()
    {
        BattleSystem.OnTargettableCharactersChanged -= ChooseAttackTarget;
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

        if (Random.value < battleSystem.ActiveEnemy.ChanceToUseSkill)
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
        battleStateManager.InitializeRandom();

        var enemy = battleSystem.ActiveEnemy;
        int attackIndex = Random.Range(0, enemy.Reference.enemyAttackAnimations.Length);
        var attack = enemy.Reference.enemyAttackAnimations[attackIndex];
        BaseCharacter.IncomingAttack = attack;

        ui.StartDefending();
        battleSystem.ActiveEnemy.BeginAttack(battleSystem.ActivePlayer.transform);
        battleSystem.ActiveEnemy.PlayAttackAnimation(attackIndex);
    }

    public void RegisterEnemyDeath(EnemyCharacter enemy)
    {
        ChooseAttacker();
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

    [IngameDebugConsole.ConsoleMethod(nameof(MaxEnemyCrit), "Set enemy character's crit chance to 100%")]
    public static void MaxEnemyCrit()
    {
        for (int i = 0; i < Instance.Enemies.Length; i++)
        {
            if (!Instance.Enemies[i]) continue;
            Instance.Enemies[i].IncreaseChargeLevel(10);
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
        }

        Debug.Log("Enemies skill use disabled!");
    }

    #endregion
}