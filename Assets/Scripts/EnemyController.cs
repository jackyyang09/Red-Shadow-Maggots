﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class EnemyController : BasicSingleton<EnemyController>
{
    public List<EnemyCharacter> Enemies { get; private set; }
    public EnemyCharacter RandomEnemy
    {
        get
        {
            return Enemies[Random.Range(0, Enemies.Count)];
        }
    }

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
        Enemies = enemyCharacters;
    }

    public void ChooseNewTargets()
    {
        ChooseAttacker();
        ChooseAttackTarget();
    }

    public void ChooseAttacker()
    {
        if (Enemies.Count == 0) return;
        battleSystem.SetEnemyAttacker(Enemies[Random.Range(0, Enemies.Count)]);
        OnChangedAttackers?.Invoke();
    }

    public void ChooseAttackTarget()
    {
        battleSystem.SetEnemyAttackTarget(BattleSystem.Instance.RandomPlayerCharacter);
        OnChangedAttackTargets?.Invoke();
    }

    public void MakeYourMove()
    {
        if (Random.value < battleSystem.ActiveEnemy.ChanceToUseSkill)
        {
            //if (enemy.CanUseSkill(index))
            battleSystem.ActivateEnemySkill(battleSystem.ActiveEnemy, Random.Range(0, 2));
        }
        else
        {
            battleSystem.BeginEnemyAttack();
        }
    }

    public void RegisterEnemyDeath(EnemyCharacter enemy)
    {
        Enemies.Remove(enemy);
        ChooseAttacker();
    }

    #region Debug Hacks
    [CommandTerminal.RegisterCommand(Help = "Instantly hurt enemies, leaving them at 1 health")]
    public static void CrippleEnemies(CommandTerminal.CommandArg[] args)
    {
        for (int i = 0; i < Instance.Enemies.Count; i++)
        {
            BaseCharacter.IncomingDamage.damage = Instance.Enemies[i].CurrentHealth - 1;
            Instance.Enemies[i].TakeDamage();
        }
        Debug.Log("Enemies damaged!");
    }
    #endregion
}