﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickTimeHold : QuickTimeBase
{
    [SerializeField] float maxLeniency = 1.5f;

    [SerializeField] protected float failZoneSize = 0.1f;

    [SerializeField] Gradient[] barGradient;

    [SerializeField] float[] barFillTimes = new float[] { 0.75f, 0.5f, 0.33f };

    [SerializeField] float barMinValue = 0.1f;

    [SerializeField] float[] barSuccessValues = new float[] { 1, 1.1f, 1.2f};

    [SerializeField] float barMissValue = 0;

    [SerializeField] int maxBarLevel = 3;
    int barLevel = 0;

    [SerializeField] float barLevelUpDelay = 0.1f;

    [SerializeField] Vector2[] gaugeRotations;
    [SerializeField] RectTransform gaugeArrow;

    [SerializeField] int successSpinCount = 2;
    [SerializeField] float successSpinTime = 1.5f;

    [SerializeField] protected Image backgroundBar;
    [SerializeField] protected Image progressBar;
    [SerializeField] protected Image fillBar;
    [SerializeField] protected Image targetBar;
    [SerializeField] JSAM.SoundFileObject chargeSound;

    Coroutine tickRoutine;

    // Update is called once per frame
    void Update()
    {
        if (tickRoutine == null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartTicking();
            }
        }
        else if (Input.GetMouseButtonUp(0) || fillBar.fillAmount == 1 && barLevel == maxBarLevel)
        {
            ExecuteAction();
        }
    }

    void BonusFeedback()
    {
        if (BaseCharacter.IncomingDamage.qteResult == QTEResult.Perfect)
        {
            backgroundBar.rectTransform.DOPunchScale(new Vector3().NewUniformVector3(0.075f), 0.25f);
            gaugeArrow.DORotate(new Vector3(0, 0, successSpinCount * -360), successSpinTime, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuart);
            ((RectTransform)gaugeArrow.parent).DOShakeAnchorPos(successSpinTime, 50, 50, 90);
        }
        else
        {
            gaugeArrow.localEulerAngles = new Vector3(0, 0, gaugeArrow.localEulerAngles.z.UnwrapAngle());
            float rotation = Mathf.Abs(gaugeArrow.localEulerAngles.z);
            gaugeArrow.DORotate(new Vector3(0, 0, 360 - rotation), successSpinTime, RotateMode.LocalAxisAdd).SetRelative();
        }
        backgroundBar.DOColor(fillBar.color, 0.1f).OnComplete(() => backgroundBar.DOColor(Color.white, 0.1f));
    }

    public void ExecuteAction()
    {
        enabled = false;
        StopCoroutine(tickRoutine);
        GetMultiplier();
        OnExecuteAnyQuickTime?.Invoke();
        OnExecuteQuickTime?.Invoke();
        Invoke(nameof(Hide), hideDelay);
        fillBar.DOKill();
        gaugeArrow.DOKill();
        BonusFeedback();
        OnQuickTimeComplete?.Invoke();
        JSAM.AudioManager.StopSound(chargeSound);
    }

    public override void InitializeBar(BaseCharacter attacker, List<BaseCharacter> target = null)
    {
        tickRoutine = null;
        barLevel = 0;
        fillBar.fillAmount = 0;
        fillBar.color = Color.white;
        gaugeArrow.localEulerAngles = new Vector3(0, 0, gaugeRotations[0].x);

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

        var failZone = progressBar.rectTransform.sizeDelta.y * failZoneSize;
        targetBar.rectTransform.anchoredPosition = new Vector2(targetBar.rectTransform.anchoredPosition.x, -Mathf.Abs(failZone));
        var maxSize = progressBar.rectTransform.sizeDelta.y - failZone;
        var lerp = Mathf.InverseLerp(0, maxLeniency, leniency);
        targetBar.rectTransform.sizeDelta = new Vector2(targetBar.rectTransform.sizeDelta.x, Mathf.Lerp(0, maxSize, lerp));
        canvas.Show();

        // Just long enough to get the users' next valid touch
        Invoke(nameof(Enable), 0.05f);
    }

    public override void StartTicking()
    {
        tickRoutine = StartCoroutine(TickRoutine());
        if (activePlayer) activePlayer.PlayAttackAnimation();
        JSAM.AudioManager.PlaySound(chargeSound);
    }

    IEnumerator TickRoutine()
    {
        while (barLevel < maxBarLevel)
        {
            fillBar.fillAmount = 0;

            fillBar.DOFillAmount(1, barFillTimes[barLevel]).SetUpdate(true).SetEase(Ease.Linear);
            fillBar.DOGradientColor(barGradient[barLevel], barFillTimes[barLevel]).SetUpdate(true).SetEase(Ease.Linear);

            gaugeArrow.DOLocalRotate(new Vector3(0, 0, gaugeRotations[barLevel].y), barFillTimes[barLevel]).SetUpdate(true).SetEase(Ease.Linear);
            yield return new WaitForSecondsRealtime(barFillTimes[barLevel]);
            fillBar.DOComplete();
            barLevel++;
            if (barLevel < maxBarLevel)
            {
                gaugeArrow.DOLocalRotate(new Vector3(0, 0, gaugeRotations[barLevel].x), barLevelUpDelay).SetEase(Ease.InSine).SetUpdate(true);
            }
            yield return new WaitForSecondsRealtime(barLevelUpDelay);
        }
    }

    public override void GetMultiplier()
    {
        float targetMin = 1 - targetBar.rectTransform.sizeDelta.y / progressBar.rectTransform.sizeDelta.y - failZoneSize;
        float targetMax = 1 - failZoneSize;

        var dmg = BaseCharacter.IncomingDamage;

        if (fillBar.fillAmount >= targetMin && fillBar.fillAmount <= targetMax)
        {
            dmg.damageNormalized = barSuccessValues[barLevel];
            dmg.damageType = DamageType.Heavy;
            dmg.qteResult = QTEResult.Perfect;
            if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn) GlobalEvents.OnPlayerQuickTimeAttackSuccess?.Invoke();
            else GlobalEvents.OnPlayerQuickTimeBlockSuccess?.Invoke();
        }
        else if (fillBar.fillAmount < targetMin)
        {
            dmg.damageNormalized = Mathf.InverseLerp(barMinValue, targetMin, fillBar.fillAmount);
            dmg.damageType = DamageType.Medium;
            dmg.qteResult = QTEResult.Early;
        }
        else
        {
            dmg.damageNormalized = barMissValue;
            dmg.damageType = DamageType.Light;
            dmg.qteResult = QTEResult.Late;
        }

        if (BattleSystem.Instance.CurrentPhase == BattlePhases.EnemyTurn)
        {
            //newStruct.damageType -= DamageType.Heavy;
            //newStruct.damageType = (DamageType)Mathf.Abs((int)newStruct.damageType);
        }

        if (AlwaysSucceed)
        {
            dmg.barFill = fillBar.fillAmount;
            dmg.damageNormalized = barSuccessValues[barLevel];
            dmg.damageType = DamageType.Heavy;
            dmg.qteResult = QTEResult.Perfect;
            dmg.chargeLevel = 2;
            if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn) GlobalEvents.OnPlayerQuickTimeAttackSuccess?.Invoke();
            else GlobalEvents.OnPlayerQuickTimeBlockSuccess?.Invoke();
        }
        else
        {
            dmg.barFill = fillBar.fillAmount;
            dmg.chargeLevel = barLevel;
        }
        BaseCharacter.IncomingDamage = dmg;
    }
}