using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuickTimeBar : QuickTimeBase
{
    [SerializeField]
    protected Image fillBar;

    [SerializeField]
    protected Image targetBar;

    [SerializeField]
    protected float maxValue;

    [SerializeField]
    protected float failZoneSize = 75;

    [SerializeField]
    protected float barSize = 625;

    [SerializeField]
    [HideInInspector]
    float BAR_WIDTH;

    [SerializeField]
    Gradient barGradient;

    [SerializeField]
    protected float barFillTime = 0.5f;

    [SerializeField]
    protected float barFillDelay = 0.5f;

    [SerializeField]
    float barMinValue = 0.1f;

    [SerializeField]
    float barSuccessValue = 1;

    [SerializeField]
    float barMissValue = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

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
        onExecuteQuickTime?.Invoke(GetMultiplier());
        Invoke("Hide", hideDelay);
        DOTween.Kill(barTweenID);
    }

    public override void StartTicking()
    {
        DOTween.To(() => fillBar.fillAmount, x => fillBar.fillAmount = x, 1, barFillTime).SetDelay(barFillDelay).intId = barTweenID;
        fillBar.DOGradientColor(barGradient, barFillTime).SetDelay(barFillDelay).intId = barTweenID;
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
            newStruct.damage = barSuccessValue;
            newStruct.damageType = DamageType.Heavy;
        }
        else if (fillBar.fillAmount < targetMin)
        {
            newStruct.damage = Mathf.InverseLerp(barMinValue, targetMin, fillBar.fillAmount);
            newStruct.damageType = DamageType.Medium;
        }
        else
        {
            newStruct.damage = barMissValue;
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