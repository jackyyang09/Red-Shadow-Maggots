using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

[CreateAssetMenu(fileName = "Battle Ally",
    menuName = "ScriptableObjects/Game Effects/Battle Ally", order = 1)]
public class BattleAllyEffect : BaseGameEffect
{
    [SerializeField] Sprite headshotGraphic;
    [SerializeField] GameObject allyPrefab;

    public float Wait() => 0.01f;
    public float WaitLimit() => 0.02f;

    public override bool Activate(AppliedEffect effect)
    {
        var e = new WaitListEntity(Wait, WaitLimit);

        e.Move = () => { };

        e.EffectRoutine = () => battleSystem.StartCoroutine(MoveRoutine(e));
        e.Headshot = headshotGraphic;
        e.Effect = effect;

        //c.OnDeath += e.RemoveSelf;

        battleSystem.AddGenericEntityToWaitList(e, true);

        allyPrefab.SetActive(false);

        effect.instantiatedObjects = new GameObject[1];
        effect.instantiatedObjects[0] = Instantiate(allyPrefab);
        var helper = effect.instantiatedObjects[0].GetComponentInChildren<BattleAllyHelper>();
        helper.Initialize(effect, this);
        helper.Attack = effect.Caster.Attack;

        return base.Activate(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        string d = skillDescription + "\n";

        var key = "$WAIT";
        d = d.Replace(key, Wait().FormatToDecimal());

        key = "$WAITLIMIT";
        d = d.Replace(key, WaitLimit().FormatToDecimal());

        return d;
    }

    public override string GetSkillDescription(EffectGroup eg)
    {
        return base.GetSkillDescription(eg);
    }

    public void InvokeAttack(BattleAllyHelper helper)
    {
        var dmg = new DamageStruct();
        dmg.TrueDamage = helper.Attack;
        dmg.QTEResult = QuickTimeBase.QTEResult.Perfect;
        dmg.QTEValue = 1;
        dmg.Effectivity = DamageEffectivess.Normal;

        if (helper.Target.IsPlayer())
        {
            dmg.QTEPlayer = 0;
            dmg.QTEEnemy = 1;
        }
        else
        {
            dmg.QTEEnemy = 1;
            dmg.QTEPlayer = 0;
        }
        helper.Target.TakeDamage(dmg);
    }

    IEnumerator MoveRoutine(WaitListEntity e)
    {
        bool isPlayer = e.Effect.Caster.IsPlayer();
        AppliedEffect effect = e.Effect;

        BaseCharacter target;

        if (isPlayer)
        {
            target = enemyController.RandomEnemy;
        }
        else
        {
            target = battleSystem.RandomLivingPlayer;
        }

        effect.instantiatedObjects[0].transform.position = target.transform.position;
        effect.instantiatedObjects[0].SetActive(true);

        var helper = effect.instantiatedObjects[0].GetComponentInChildren<BattleAllyHelper>();
        helper.Target = target;
        helper.AnimFinished = false;

        yield return new WaitUntil(() => helper.AnimFinished);

        effect.instantiatedObjects[0].SetActive(false);

        battleSystem.EndTurn();
    }
}