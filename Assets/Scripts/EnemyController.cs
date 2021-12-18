using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class EnemyController : MonoBehaviour
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

    public static EnemyController Instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

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
        battleSystem.BeginEnemyAttack();
        //battleSystem.ActivateEnemySkill(battleSystem.ActiveEnemy, Random.Range(0, 2));
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
            DamageStruct dmg = new DamageStruct();
            dmg.damage = Instance.Enemies[i].CurrentHealth - 1;
            Instance.Enemies[i].TakeDamage(dmg);
        }
        Debug.Log("Enemies damaged!");
    }
    #endregion

    void EstablishSingletonDominance()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            // A unique case where the Singleton exists but not in this scene
            if (Instance.gameObject.scene.name == null)
            {
                Instance = this;
            }
            else if (!Instance.gameObject.activeInHierarchy)
            {
                Instance = this;
            }
            else if (Instance.gameObject.scene.name != gameObject.scene.name)
            {
                Instance = this;
            }
            Destroy(gameObject);
        }
    }
}
