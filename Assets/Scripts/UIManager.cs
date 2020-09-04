using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    OptimizedButton attackButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AttackPress()
    {
        attackButton.Hide();
        BattleSystem.instance.ExecutePlayerAttack();
    }
}
