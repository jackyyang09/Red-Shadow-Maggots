using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

[CreateAssetMenu(fileName = "Battle Ally",
    menuName = "ScriptableObjects/Game Effects/Battle Ally", order = 1)]
public class BattleAllyEffect : BaseGameEffect
{
    [SerializeField] Sprite headshotGraphic;

    public float Wait() => 50;
    public float WaitLimit() => 100;

    public override bool Activate(AppliedEffect effect)
    {
        var e = new WaitListEntity(Wait, WaitLimit);

        e.Move = () => { };

        e.EffectRoutine = MoveRoutine();
        e.Headshot = headshotGraphic;

        //c.OnDeath += e.RemoveSelf;

        battleSystem.AddGenericEntityToWaitList(e);

        return base.Activate(effect);
    }

    IEnumerator MoveRoutine()
    {
        Debug.Log("Hey whatup");

        yield return new WaitForSeconds(2);

        Debug.Log("Ok lol i'm done");

        battleSystem.Invoke("EndTurn", 1);
    }
}