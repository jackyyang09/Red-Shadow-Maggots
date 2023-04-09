using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static Facade;

public class QuickTimeBar : QuickTimeBase
{
    [SerializeField] Image backgroundBar;
    [SerializeField] Image progressBar;
    [SerializeField] Image fillBar;
    [SerializeField] Image targetBar;

    [SerializeField] float maxLeniency = 1.5f;

    [SerializeField] float failZoneSize = 0.1f;

    [SerializeField] Gradient barGradient = null;

    [SerializeField] protected float barFillTime = 0.5f;

    [SerializeField] protected float barFillDelay = 0.5f;

    [SerializeField] float barMinValue = 0.1f;

    [SerializeField] float barSuccessValue = 1;

    [SerializeField] float barMissValue = 0;

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

        fillBar.DOGradientColor(barGradient, barFillTime).SetUpdate(true).SetDelay(barFillDelay).SetEase(Ease.Linear);
        Invoke(nameof(Enable), barFillDelay);
    }

    public override void InitializeBar(BaseCharacter attacker, List<BaseCharacter> target = null)
    {
        fillBar.fillAmount = 0;
        fillBar.color = Color.white;

        float leniency = 0;
        switch (battleSystem.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                activePlayer = attacker as PlayerCharacter;
                leniency = attacker.AttackLeniency;
                break;
            case BattlePhases.EnemyTurn:
                leniency = attacker.DefenseLeniency;
                break;
        }

        var failZone = progressBar.rectTransform.sizeDelta.x * failZoneSize;
        targetBar.rectTransform.anchoredPosition = new Vector2(-Mathf.Abs(failZone), targetBar.rectTransform.anchoredPosition.y);
        var maxSize = progressBar.rectTransform.sizeDelta.x - failZone;
        var lerp = Mathf.InverseLerp(0, maxLeniency, leniency);
        targetBar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, maxSize, lerp), targetBar.rectTransform.sizeDelta.y);
        canvas.Show();
        StartTicking();
    }

    public override void GetMultiplier()
    {
        float targetMin = 1 - targetBar.rectTransform.sizeDelta.x / progressBar.rectTransform.sizeDelta.x + failZoneSize;
        float targetMax = 1 - failZoneSize;

        var dmg = BaseCharacter.IncomingDamage;
        if (fillBar.fillAmount >= targetMin && fillBar.fillAmount <= targetMax)
        {
            dmg.damageNormalized = barSuccessValue;
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

        if (battleSystem.CurrentPhase == BattlePhases.EnemyTurn)
        {
            dmg.damageType -= DamageType.Heavy;
            dmg.damageType = (DamageType)Mathf.Abs((int)BaseCharacter.IncomingDamage.damageType);
        }

        dmg.barFill = fillBar.fillAmount;
        BaseCharacter.IncomingDamage = dmg;
    }
}