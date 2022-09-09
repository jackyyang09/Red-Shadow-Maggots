using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OpenCasketFuneral : SuperCriticalEffect
{
    [SerializeField] Transform skeletonTransform = null;
    [SerializeField] Renderer skeletonArms = null;
    int turnsPassed;

    private void OnDisable()
    {
        if (turnsPassed < 3)
        {
            BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] -= TickTurnsPassed;
        }
    }

    public override void BeginSuperCritEffect()
    {
        turnsPassed = 0;
        skeletonTransform.transform.localScale = Vector3.one;
        BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] += TickTurnsPassed;
        skeletonArms.enabled = true;
    }

    public override void FinishSuperCritEffect()
    {
        base.FinishSuperCritEffect();
        animHelper.DisableCrits();
    }

    private void TickTurnsPassed()
    {
        turnsPassed++;
        if (turnsPassed == 3)
        {
            skeletonTransform.transform.DOScale(0.01f, 0.5f).SetUpdate(UpdateType.Late).OnComplete(() =>
            {
                skeletonArms.enabled = false;
            });
            BattleSystem.OnStartPhase[BattlePhases.PlayerTurn.ToInt()] -= TickTurnsPassed;
        }
    }
}