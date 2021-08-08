using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public List<EnemyCharacter> enemies { get; private set; }
    public EnemyCharacter RandomEnemy
    {
        get
        {
            return enemies[Random.Range(0, enemies.Count)];
        }
    }

    public static EnemyController instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //
    //}

    // Update is called once per frame
    //void Update()
    //{
    //
    //}

    public void AssignEnemies(List<EnemyCharacter> enemyCharacters)
    {
        enemies = enemyCharacters;
    }

    public void MakeYourMove()
    {
        BattleSystem.Instance.SetActiveEnemy(enemies[Random.Range(0, enemies.Count)]);
        BattleSystem.Instance.SetActivePlayer(BattleSystem.Instance.RandomPlayerCharacter);
        BattleSystem.Instance.ExecuteEnemyAttack();
    }

    public void RegisterEnemyDeath(EnemyCharacter enemy)
    {
        enemies.Remove(enemy);
    }

    #region Debug Hacks
    [CommandTerminal.RegisterCommand(Help = "Instantly hurt enemies, leaving them at 1 health")]
    public static void CrippleEnemies(CommandTerminal.CommandArg[] args)
    {
        for (int i = 0; i < instance.enemies.Count; i++)
        {
            DamageStruct dmg = new DamageStruct();
            dmg.damage = instance.enemies[i].CurrentHealth - 1;
            instance.enemies[i].TakeDamage(dmg);
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
