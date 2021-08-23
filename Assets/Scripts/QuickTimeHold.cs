using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuickTimeHold : QuickTimeBase
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
    [SerializeField] float BAR_HEIGHT = 0;

    [SerializeField] Gradient[] barGradient = null;

    [SerializeField] float[] barFillTimes = new float[] { 0.75f, 0.5f, 0.33f };

    [SerializeField] float barMinValue = 0.1f;

    [SerializeField] float[] barSuccessValues = new float[] { 1, 1.1f, 1.2f};

    [SerializeField] float barMissValue = 0;

    [SerializeField] int maxBarLevel = 3;
    int barLevel = 0;

    [SerializeField] float barLevelUpDelay = 0.1f;

    [SerializeField] Vector2[] gaugeRotations = null;
    [SerializeField] RectTransform gaugeArrow = null;

    [SerializeField] int successSpinCount = 2;
    [SerializeField] float successSpinTime = 1.5f;

    [SerializeField] JSAM.JSAMSoundFileObject chargeSound = null;

    Coroutine tickRoutine = null;

    private void OnValidate()
    {
        targetBar.rectTransform.anchoredPosition = new Vector2(targetBar.rectTransform.anchoredPosition.x, - Mathf.Abs(failZoneSize));
        BAR_HEIGHT = failZoneSize + barSize;
    }

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

    void BonusFeedback(DamageStruct damage)
    {
        if (damage.qteResult == QTEResult.Perfect)
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
        StopCoroutine(tickRoutine);
        DamageStruct dmg = GetMultiplier();
        onExecuteQuickTime?.Invoke(dmg);
        Invoke("Hide", hideDelay);
        fillBar.DOKill();
        gaugeArrow.DOKill();
        BonusFeedback(dmg);
        OnQuickTimeComplete?.Invoke(dmg);
        JSAM.AudioManager.StopSound(chargeSound);
    }

    public override void InitializeBar(PlayerCharacter player)
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
                activePlayer = player;
                leniency = player.GetAttackLeniency();
                break;
            case BattlePhases.EnemyTurn:
                leniency = player.GetDefenceLeniency();
                break;
        }

        targetBar.rectTransform.sizeDelta = new Vector2(targetBar.rectTransform.sizeDelta.x, Mathf.Lerp(0, maxValue, leniency));
        canvas.Show();

        // Just long enough to get the users' next valid touch
        Invoke("Enable", 0.05f);
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
            float targetMin = 1 - (targetBar.rectTransform.sizeDelta.y + failZoneSize) / BAR_HEIGHT;
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

    public override DamageStruct GetMultiplier()
    {
        float targetMin = 1 - (targetBar.rectTransform.sizeDelta.x + failZoneSize) / BAR_HEIGHT;
        float targetMax = barSize / BAR_HEIGHT;

        DamageStruct damageStruct = new DamageStruct();

        if (fillBar.fillAmount >= targetMin && fillBar.fillAmount <= targetMax)
        {
            damageStruct.damageNormalized = barSuccessValues[barLevel];
            damageStruct.damageType = DamageType.Heavy;
            damageStruct.qteResult = QTEResult.Perfect;
            if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn) GlobalEvents.OnPlayerQuickTimeAttackSuccess?.Invoke();
            else GlobalEvents.OnPlayerQuickTimeBlockSuccess?.Invoke();
        }
        else if (fillBar.fillAmount < targetMin)
        {
            damageStruct.damageNormalized = Mathf.InverseLerp(barMinValue, targetMin, fillBar.fillAmount);
            damageStruct.damageType = DamageType.Medium;
            damageStruct.qteResult = QTEResult.Early;
        }
        else
        {
            damageStruct.damageNormalized = barMissValue;
            damageStruct.damageType = DamageType.Light;
            damageStruct.qteResult = QTEResult.Late;
        }

        if (BattleSystem.Instance.CurrentPhase == BattlePhases.EnemyTurn)
        {
            //newStruct.damageType -= DamageType.Heavy;
            //newStruct.damageType = (DamageType)Mathf.Abs((int)newStruct.damageType);
        }

        damageStruct.barFill = fillBar.fillAmount;
        damageStruct.chargeLevel = barLevel;

        return damageStruct;
    }
}