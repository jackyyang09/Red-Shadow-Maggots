using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuickTimeBar : QuickTimeBase
{
    [SerializeField] protected Image backgroundBar = null;

    [SerializeField] protected Image fillBar = null;

    [SerializeField] protected Image targetBar = null;

    [SerializeField]
    protected float maxValue = 0;

    [SerializeField]
    protected float failZoneSize = 75;

    [SerializeField]
    protected float barSize = 625;

    [HideInInspector]
    [SerializeField] float BAR_WIDTH = 0;

    [SerializeField] Gradient barGradient = null;

    [SerializeField] protected float barFillTime = 0.5f;

    [SerializeField] protected float barFillDelay = 0.5f;

    [SerializeField] float barMinValue = 0.1f;

    [SerializeField] float barSuccessValue = 1;

    [SerializeField] float barMissValue = 0;

    private void OnValidate()
    {
        targetBar.rectTransform.anchoredPosition = new Vector2(-Mathf.Abs(failZoneSize), targetBar.rectTransform.anchoredPosition.y);
        BAR_WIDTH = failZoneSize + barSize;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || fillBar.fillAmount == 1)
        {
            ExecuteAction();
        }
    }

    public void ExecuteAction()
    {
        enabled = false;
        DamageStruct dmg = GetMultiplier();
        onExecuteQuickTime?.Invoke(dmg);
        BonusFeedback(dmg);
        Invoke("Hide", hideDelay);
        fillBar.DOKill();
    }

    void BonusFeedback(DamageStruct damage)
    {
        if (damage.quickTimeSuccess)
        {
            backgroundBar.rectTransform.DOPunchScale(new Vector3().NewUniformVector3(0.075f), 0.25f);
        }
        backgroundBar.DOColor(fillBar.color, 0.1f).OnComplete(() => backgroundBar.DOColor(Color.white, 0.1f));
    }

    public override void StartTicking()
    {
        if (activePlayer) activePlayer.PlayAttackAnimation();
        fillBar.DOFillAmount(1, barFillTime).SetUpdate(true).SetDelay(barFillDelay).SetEase(Ease.Linear);

        float targetMin = 1 - (targetBar.rectTransform.sizeDelta.x + failZoneSize) / BAR_WIDTH;
        //fillBar.DOGradientColor(barGradient, Mathf.Lerp(0, barFillTime, targetMin)).SetUpdate(true).SetDelay(barFillDelay);
        fillBar.DOGradientColor(barGradient, barFillTime).SetUpdate(true).SetDelay(barFillDelay).SetEase(Ease.Linear);
        Invoke("Enable", barFillDelay);
    }

    public override void InitializeBar(PlayerCharacter player)
    {
        fillBar.fillAmount = 0;
        fillBar.color = Color.white;

        float leniency = 0;
        switch (BattleSystem.instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                activePlayer = player;
                leniency = player.GetAttackLeniency();
                break;
            case BattlePhases.EnemyTurn:
                leniency = player.GetDefenceLeniency();
                break;
        }

        targetBar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, maxValue, leniency), targetBar.rectTransform.sizeDelta.y);
        canvas.Show();
        StartTicking();
    }

    public override DamageStruct GetMultiplier()
    {
        float targetMin = 1 - (targetBar.rectTransform.sizeDelta.x + failZoneSize) / BAR_WIDTH;
        float targetMax = barSize / BAR_WIDTH;

        DamageStruct newStruct = new DamageStruct();

        if (fillBar.fillAmount >= targetMin && fillBar.fillAmount <= targetMax)
        {
            newStruct.damageNormalized = barSuccessValue;
            newStruct.damageType = DamageType.Heavy;
            newStruct.quickTimeSuccess = true;
            if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn) GlobalEvents.OnPlayerQuickTimeAttackSuccess?.Invoke();
            else GlobalEvents.OnPlayerQuickTimeBlockSuccess?.Invoke();
        }
        else if (fillBar.fillAmount < targetMin)
        {
            newStruct.damageNormalized = Mathf.InverseLerp(barMinValue, targetMin, fillBar.fillAmount);
            newStruct.damageType = DamageType.Medium;
        }
        else
        {
            newStruct.damageNormalized = barMissValue;
            newStruct.damageType = DamageType.Light;
        }

        if (BattleSystem.instance.CurrentPhase == BattlePhases.EnemyTurn)
        {
            newStruct.damageType -= DamageType.Heavy;
            newStruct.damageType = (DamageType)Mathf.Abs((int)newStruct.damageType);
        }

        return newStruct;
    }
}