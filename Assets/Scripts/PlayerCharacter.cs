using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : BaseCharacter
{

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAttackAnimation()
    {

    }

    private void OnMouseDown()
    {
        BattleSystem.instance.PlayerSelectCharacter(this);
    }
}
