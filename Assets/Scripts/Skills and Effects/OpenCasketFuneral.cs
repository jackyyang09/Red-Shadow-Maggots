using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OpenCasketFuneral : MonoBehaviour
{
    [SerializeField] Transform skeletonTransform = null;
    [SerializeField] Renderer skeletonArms = null;
    int turnsPassed;

    private void OnDisable()
    {
        if (turnsPassed < 3)
        {
            BattleSystem.OnStartPlayerTurn -= TickTurnsPassed;
        }
    }

    public void ShowArms()
    {
        turnsPassed = 0;
        skeletonTransform.transform.localScale = Vector3.one;
        BattleSystem.OnStartPlayerTurn += TickTurnsPassed;
        skeletonArms.enabled = true;
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
            BattleSystem.OnStartPlayerTurn -= TickTurnsPassed;
        }
    }
}
