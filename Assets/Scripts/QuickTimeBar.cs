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

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

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

    int barTweenID = 33;
    public void ExecuteAction()
    {
        enabled = false;
        DamageStruct dmg = GetMultiplier();
        onExecuteQuickTime?.Invoke(dmg);
        BonusFeedback(dmg);
        Invoke("Hide", hideDelay);
        DOTween.Kill(barTweenID);
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
        DOTween.To(() => fillBar.fillAmount, x => fillBar.fillAmount = x, 1, barFillTime).SetUpdate(true).SetDelay(barFillDelay).intId = barTweenID;

        float targetMin = 1 - (targetBar.rectTransform.sizeDelta.x + failZoneSize) / BAR_WIDTH;
        fillBar.DOGradientColor(barGradient, Mathf.Lerp(0, barFillTime, targetMin)).SetUpdate(true).SetDelay(barFillDelay).intId = barTweenID;
        Invoke("Enable", barFillDelay);
    }

    public override void InitializeBar(float leniency)
    {
        fillBar.fillAmount = 0;
        fillBar.color = Color.white;
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
            if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn) GlobalEvents.onPlayerQuickTimeAttackSuccess?.Invoke();
            else GlobalEvents.onPlayerQuickTimeBlockSuccess?.Invoke();
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