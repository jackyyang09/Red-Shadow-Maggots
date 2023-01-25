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
        GetMultiplier();
        onExecuteQuickTime?.Invoke();
        OnExecuteQuickTime?.Invoke();
        BonusFeedback();
        Invoke(nameof(Hide), hideDelay);
        fillBar.DOKill();
        OnQuickTimeComplete?.Invoke();
    }

    void BonusFeedback()
    {
        if (BaseCharacter.IncomingDamage.qteResult == QTEResult.Perfect)
        {
            backgroundBar.rectTransform.DOPunchScale(new Vector3().NewUniformVector3(0.075f), 0.25f);
        }
        backgroundBar.DOColor(fillBar.color, 0.1f).OnComplete(() => backgroundBar.DOColor(Color.white, 0.1f));
    }

    public override void StartTicking()
    {
        fillBar.DOFillAmount(1, barFillTime).SetUpdate(true).SetDelay(barFillDelay).SetEase(Ease.Linear);

        float targetMin = 1 - (targetBar.rectTransform.sizeDelta.x + failZoneSize) / BAR_WIDTH;
        //fillBar.DOGradientColor(barGradient, Mathf.Lerp(0, barFillTime, targetMin)).SetUpdate(true).SetDelay(barFillDelay);
        fillBar.DOGradientColor(barGradient, barFillTime).SetUpdate(true).SetDelay(barFillDelay).SetEase(Ease.Linear);
        Invoke(nameof(Enable), barFillDelay);
    }

    public override void InitializeBar(BaseCharacter attacker, List<BaseCharacter> target = null)
    {
        fillBar.fillAmount = 0;
        fillBar.color = Color.white;

        float leniency = 0;
        switch (BattleSystem.Instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                activePlayer = attacker as PlayerCharacter;
                leniency = attacker.AttackLeniency;
                break;
            case BattlePhases.EnemyTurn:
                leniency = attacker.DefenseLeniency;
                break;
        }

        targetBar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, maxValue, leniency), targetBar.rectTransform.sizeDelta.y);
        canvas.Show();
        StartTicking();
    }

    public override void GetMultiplier()
    {
        float targetMin = 1 - (targetBar.rectTransform.sizeDelta.x + failZoneSize) / BAR_WIDTH;
        float targetMax = barSize / BAR_WIDTH;

        if (fillBar.fillAmount >= targetMin && fillBar.fillAmount <= targetMax)
        {
            BaseCharacter.IncomingDamage.damageNormalized = barSuccessValue;
            BaseCharacter.IncomingDamage.damageType = DamageType.Heavy;
            BaseCharacter.IncomingDamage.qteResult = QTEResult.Perfect;
            if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn) GlobalEvents.OnPlayerQuickTimeAttackSuccess?.Invoke();
            else GlobalEvents.OnPlayerQuickTimeBlockSuccess?.Invoke();
        }
        else if (fillBar.fillAmount < targetMin)
        {
            BaseCharacter.IncomingDamage.damageNormalized = Mathf.InverseLerp(barMinValue, targetMin, fillBar.fillAmount);
            BaseCharacter.IncomingDamage.damageType = DamageType.Medium;
            BaseCharacter.IncomingDamage.qteResult = QTEResult.Early;
        }
        else
        {
            BaseCharacter.IncomingDamage.damageNormalized = barMissValue;
            BaseCharacter.IncomingDamage.damageType = DamageType.Light;
            BaseCharacter.IncomingDamage.qteResult = QTEResult.Late;
        }

        if (BattleSystem.Instance.CurrentPhase == BattlePhases.EnemyTurn)
        {
            BaseCharacter.IncomingDamage.damageType -= DamageType.Heavy;
            BaseCharacter.IncomingDamage.damageType = (DamageType)Mathf.Abs((int)BaseCharacter.IncomingDamage.damageType);
        }

        BaseCharacter.IncomingDamage.barFill = fillBar.fillAmount;
    }
}