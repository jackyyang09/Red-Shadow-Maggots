using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static EnemyController instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    //void Update()
    //{
    //
    //}

    public void AssignEnemies(List<EnemyCharacter> enemyCharacters)
    {
        Enemies = enemyCharacters;
    }

    public void MakeYourMove()
    {
        BattleSystem.Instance.SetActiveEnemy(Enemies[Random.Range(0, Enemies.Count)]);
        BattleSystem.Instance.SetActivePlayer(BattleSystem.Instance.RandomPlayerCharacter);
        BattleSystem.Instance.ExecuteEnemyAttack();
    }

    public void RegisterEnemyDeath(EnemyCharacter enemy)
    {
        Enemies.Remove(enemy);
    }

    #region Debug Hacks
    [CommandTerminal.RegisterCommand(Help = "Instantly hurt enemies, leaving them at 1 health")]
    public static void CrippleEnemies(CommandTerminal.CommandArg[] args)
    {
        for (int i = 0; i < instance.Enemies.Count; i++)
        {
            DamageStruct dmg = new DamageStruct();
            dmg.damage = instance.Enemies[i].CurrentHealth - 1;
            instance.Enemies[i].TakeDamage(dmg);
        }
        Debug.Log("Enemies damaged!");
    }
    #endregion

    void EstablishSingletonDominance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // A unique case where the Singleton exists but not in this scene
            if (instance.gameObject.scene.name == null)
            {
                instance = this;
            }
            else if (!instance.gameObject.activeInHierarchy)
            {
                instance = this;
            }
            else if (instance.gameObject.scene.name != gameObject.scene.name)
            {
                instance = this;
            }
            Destroy(gameObject);
        }
    }
}
