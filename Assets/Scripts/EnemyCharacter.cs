using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : BaseCharacter
{
    [SerializeField]
    UnityEngine.UI.Image selectionPointer;

    public static System.Action<EnemyCharacter> onSelectedEnemyCharacterChange;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        onSelectedEnemyCharacterChange += UpdateSelectedStatus;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        onSelectedEnemyCharacterChange -= UpdateSelectedStatus;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateSelectedStatus(EnemyCharacter selectedEnemy)
    {
        if (selectedEnemy == this)
        {
            ShowCharacterUI();
        }
        else
        {
            HideCharacterUI();
        }
    }

    public override void ShowCharacterUI()
    {
        bool isSelected = BattleSystem.instance.GetActiveEnemy() == this;
        selectionPointer.enabled = isSelected;
        anim.SetBool("Selected", isSelected);
    }

    public override void HideCharacterUI()
    {
        selectionPointer.enabled = false;
        anim.SetBool("Selected", false);
    }

    private void OnMouseDown()
    {
        if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn && UIManager.CanSelect)
        {
            GlobalEvents.onSelectCharacter?.Invoke(this);
            if (BattleSystem.instance.GetActiveEnemy() == this) return;
            BattleSystem.instance.SetActiveEnemy(this);
            onSelectedEnemyCharacterChange?.Invoke(this);
        }
    }

    public override void Die()
    {
        EnemyController.instance.RegisterEnemyDeath(this);
        Invoke("DeathEffects", 0.5f);
    }

    public void DeathEffects()
    {
        onDeath?.Invoke();
        EnemyController.instance.RegisterEnemyDeath(this);
        GlobalEvents.onAnyEnemyDeath?.Invoke();
        GlobalEvents.onCharacterDeath?.Invoke(this);
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        anim.SetTrigger("Death");
        //Destroy(gameObject);
    }
}