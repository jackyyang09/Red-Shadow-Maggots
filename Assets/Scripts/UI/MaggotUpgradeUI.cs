using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using EasingCurve;
using TMPro;
using static Facade;

public class MaggotUpgradeUI : BasicSingleton<MaggotUpgradeUI>
{
    [SerializeField] EasingFunctions.Ease upgradeAnimationEase;
    AnimationCurve upgradeAnimCurve;
    [SerializeField] float upgradeAnimationTime = 1;

    [SerializeField] OptimizedCanvas canvas;
    public OptimizedCanvas OptimizedCanvas => canvas;

    [SerializeField] CardCanvasProjection projection;
    Transform CardHolder => projection.transform;

    [SerializeField]
    TextMeshProUGUI
        scrapOfferedLabel,
        scrapOwnedLabel,
        currentHealthLabel,
        healthArrow,
        nextHealthLabel,
        currentAttackLabel,
        attackArrow,
        nextAttackLabel,
        levelChangeLabel,
        expBarLabel;
    [SerializeField] Image currentExpFill, nextExpFill;
    [SerializeField] OptimizedButton plusButton, minusButton;
    [SerializeField] Image raycastMask;

    /// <summary>
    /// TODO: Put this somewhere else
    /// </summary>
    int expMultiplier = 10;
    int scrapOffered;
    int ScrapOffered
    {
        set
        {
            scrapOffered = value;
            scrapOfferedLabel.enabled = value > 0;
            scrapOfferedLabel.text = scrapOffered.ToString();
        }
        get => scrapOffered;
    }

    CharacterObject TargetCharacter => projection.CardHolder.Character;
    PlayerSave.MaggotState maggotState;

    private void OnValidate()
    {
        upgradeAnimCurve = EasingAnimationCurve.EaseToAnimationCurve(upgradeAnimationEase);
    }

    public void InitializeUI(CharacterObject character, PlayerSave.MaggotState state)
    {
        maggotState = state;

        projection.CardHolder.SetCharacterAndRarity(character, gachaSystem.RandomRarity);

        RefreshUI();
    }

    void RefreshUI()
    {
        var data = playerDataManager.LoadedData;

        scrapOfferedLabel.enabled = false;
        scrapOwnedLabel.text = "x" + data.Exp.ToString();

        int currentLevel = TargetCharacter.GetLevelFromExp(maggotState.Exp);
        currentHealthLabel.text = TargetCharacter.GetMaxHealth(currentLevel, false).ToString();
        healthArrow.enabled = false;
        nextHealthLabel.text = "";
        currentAttackLabel.text = TargetCharacter.GetAttack(currentLevel).ToString();
        attackArrow.enabled = false;
        nextAttackLabel.text = "";

        float expProgress = maggotState.Exp - TargetCharacter.GetExpRequiredForLevel(0, currentLevel);
        float expRequirement = TargetCharacter.GetExpRequiredForLevel(currentLevel, currentLevel + 1);

        levelChangeLabel.text = "Lvl " + currentLevel;
        expBarLabel.text = (int)expProgress + "/" + (int)expRequirement;

        currentExpFill.fillAmount = Mathf.Round(expProgress) / expRequirement;
        nextExpFill.enabled = false;

        if (playerDataManager.LoadedData.Exp == 0)
        {
            plusButton.SetButtonInteractable(false);
        }

        minusButton.SetButtonInteractable(false);
    }

    void UpdateUIElements()
    {
        int currentLevel = TargetCharacter.GetLevelFromExp(maggotState.Exp);
        float currentExp = maggotState.Exp - TargetCharacter.GetExpRequiredForLevel(0, currentLevel);
        float incomingExp = maggotState.Exp + ScrapOffered * expMultiplier;
        int incomingLevel = TargetCharacter.GetLevelFromExp(incomingExp);

        float expProgress = incomingExp - TargetCharacter.GetExpRequiredForLevel(0, incomingLevel);
        float expRequirement = TargetCharacter.GetExpRequiredForLevel(incomingLevel, incomingLevel + 1);

        levelChangeLabel.text = "Lvl " + currentLevel;
        expBarLabel.text = (int)expProgress + "/" + (int)expRequirement;

        currentExpFill.enabled = incomingLevel <= currentLevel;
        if (currentExpFill.enabled)
        {
            currentExpFill.fillAmount = Mathf.Round(currentExp) / expRequirement;
        }
        else
        {
            levelChangeLabel.text += " -> <color=#EFD026>" + incomingLevel + "</color>";
            nextHealthLabel.text = TargetCharacter.GetMaxHealth(incomingLevel, false).ToString();
            nextAttackLabel.text = TargetCharacter.GetAttack(incomingLevel).ToString();
        }

        healthArrow.enabled = !currentExpFill.enabled;
        nextHealthLabel.enabled = !currentExpFill.enabled;
        attackArrow.enabled = !currentExpFill.enabled;
        nextAttackLabel.enabled = !currentExpFill.enabled;

        nextExpFill.enabled = ScrapOffered > 0;
        nextExpFill.fillAmount = Mathf.Round(expProgress) / expRequirement;
    }

    public void OfferScrap()
    {
        ScrapOffered++;

        minusButton.SetButtonInteractable(true);

        UpdateUIElements();

        if (ScrapOffered >= playerDataManager.LoadedData.Exp)
        {
            plusButton.SetButtonInteractable(false);
        }
    }

    public void MinusScrap()
    {
        ScrapOffered--;

        UpdateUIElements();

        if (ScrapOffered == 0)
        {
            minusButton.SetButtonInteractable(false);
        }
        else
        {
            plusButton.SetButtonInteractable(true);
        }
    }

    public void ResetScrap()
    {
        ScrapOffered = 0;

        UpdateUIElements();
        minusButton.SetButtonInteractable(false);
        plusButton.SetButtonInteractable(true);
    }

    public void Upgrade()
    {
        StartCoroutine(UpgradeAnimation());
    }
    
    IEnumerator UpgradeAnimation()
    {
        int startLevel = TargetCharacter.GetLevelFromExp(maggotState.Exp);
        int currentLevel = startLevel;
        float currentExp = maggotState.Exp;
        float incomingExp = ScrapOffered * expMultiplier;
        float targetExp = maggotState.Exp + incomingExp;

        nextExpFill.enabled = false;
        currentExpFill.enabled = true;

        yield return null;

        raycastMask.enabled = true;

        float timer = 0;
        while (timer < upgradeAnimationTime)
        {
            timer += Time.deltaTime;

            var xp = Mathf.Lerp(currentExp, targetExp, upgradeAnimCurve.Evaluate(timer / upgradeAnimationTime));

            currentLevel = TargetCharacter.GetLevelFromExp(xp);
            xp -= TargetCharacter.GetExpRequiredForLevel(0, currentLevel);
            var requiredExp = TargetCharacter.GetExpRequiredForLevel(currentLevel, currentLevel + 1);
            currentExpFill.fillAmount = xp / requiredExp;

            expBarLabel.text = (int)xp + "/" + (int)requiredExp;

            yield return null;
        }

        CardHolder.DOLocalMoveZ(-0.5f, inTime).SetEase(inEase);

        yield return new WaitForSeconds(inAndSpin);

        CardHolder.DOLocalRotate(new Vector3(0, spinAmount, 0), spinTime, RotateMode.LocalAxisAdd)
            .SetEase(spinEase);

        yield return new WaitForSeconds(spinAndOut);

        CardHolder.DOLocalMoveZ(0, outTime).SetEase(outEase);

        yield return new WaitForSeconds(spinTime);

        CardHolder.localEulerAngles = Vector3.zero;

        raycastMask.enabled = false;

        if (!dontConfirmUpgrade)
        {
            var healthPercent = maggotState.Health / TargetCharacter.GetMaxHealth(startLevel, false);
            maggotState.Exp = targetExp;
            playerDataManager.SetExp(playerDataManager.LoadedData.Exp - ScrapOffered);
            var newLevel = TargetCharacter.GetLevelFromExp(maggotState.Exp);
            maggotState.Health = healthPercent * TargetCharacter.GetMaxHealth(newLevel, false);

            ScrapOffered = 0;
            RefreshUI();
        }
    }

    public void ReturnToMaggotSelect()
    {
        cardListUI.ShowUI();
        canvas.Hide();
    }

    public void FinishUpgrade()
    {
        mapManager.SaveMap();
        playerDataManager.SaveData();
        cardListUI.ShowUI();
        canvas.Hide();
        cardListUI.HideUI();
        restNode.FinishUpgrade();
    }

    [Header(("Animation Debug"))]
    [SerializeField] bool dontConfirmUpgrade = false;

    [SerializeField] Ease inEase = Ease.Linear;
    [SerializeField] float inTime = 0.25f;
    [SerializeField] float inAndSpin = 0;
    [SerializeField] Ease moveEase = Ease.Linear;
    [SerializeField] float spinAmount = -360;
    [SerializeField] Ease spinEase = Ease.Linear;
    [SerializeField] float spinTime = 0.25f;
    [SerializeField] float spinAndOut = 0;
    [SerializeField] Ease outEase = Ease.Linear;
    [SerializeField] float outTime = 0.25f;
}