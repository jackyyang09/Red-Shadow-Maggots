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
    public OptimizedCanvas OptimizedCanvas { get { return canvas; } }

    [SerializeField] Transform cardHolderParent;
    Transform cardHolder;

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
    [SerializeField] Button plusButton, minusButton;
    [SerializeField] GraphicRaycaster plusRaycaster, minusRaycaster;
    [SerializeField] Image raycastMask;
    [SerializeField] Button backButton, doneButton;

    /// <summary>
    /// TODO: Put this somewhere else
    /// </summary>
    int expMultiplier = 30;
    int scrapOffered;

    CharacterObject targetCharacter;
    PlayerSave.MaggotState maggotState;

    private void OnValidate()
    {
        upgradeAnimCurve = EasingAnimationCurve.EaseToAnimationCurve(upgradeAnimationEase);
    }

    public void InitializeUI(CharacterCardHolder holder, PlayerSave.MaggotState state)
    {
        maggotState = state;

        cardHolder = holder.transform;
        holder.transform.SetParent(cardHolderParent);
        holder.transform.localPosition = Vector3.zero;

        targetCharacter = holder.Character;

        RefreshUI();
    }

    void RefreshUI()
    {
        var data = playerDataManager.LoadedData;

        scrapOfferedLabel.enabled = false;
        scrapOwnedLabel.text = "x" + data.Exp.ToString();

        int currentLevel = targetCharacter.GetLevelFromExp(maggotState.Exp);
        currentHealthLabel.text = targetCharacter.GetMaxHealth(currentLevel, false).ToString();
        healthArrow.enabled = false;
        nextHealthLabel.text = "";
        currentAttackLabel.text = targetCharacter.GetAttack(currentLevel).ToString();
        attackArrow.enabled = false;
        nextAttackLabel.text = "";

        float expProgress = maggotState.Exp - targetCharacter.GetExpRequiredForLevel(0, currentLevel);
        float expRequirement = targetCharacter.GetExpRequiredForLevel(currentLevel, currentLevel + 1);

        levelChangeLabel.text = "Lvl " + currentLevel;
        expBarLabel.text = (int)expProgress + "/" + (int)expRequirement;

        currentExpFill.fillAmount = Mathf.Round(expProgress) / expRequirement;
        nextExpFill.enabled = false;

        if (playerDataManager.LoadedData.Exp == 0)
        {
            plusButton.interactable = false;
            plusRaycaster.enabled = false;
        }

        minusButton.interactable = false;
        minusRaycaster.enabled = false;
    }

    void UpdateUIElements()
    {
        int currentLevel = targetCharacter.GetLevelFromExp(maggotState.Exp);
        float currentExp = maggotState.Exp - targetCharacter.GetExpRequiredForLevel(0, currentLevel);
        float incomingExp = maggotState.Exp + scrapOffered * expMultiplier;
        int incomingLevel = targetCharacter.GetLevelFromExp(incomingExp);

        float expProgress = incomingExp - targetCharacter.GetExpRequiredForLevel(0, incomingLevel);
        float expRequirement = targetCharacter.GetExpRequiredForLevel(incomingLevel, incomingLevel + 1);

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
            nextHealthLabel.text = targetCharacter.GetMaxHealth(incomingLevel, false).ToString();
            nextAttackLabel.text = targetCharacter.GetAttack(incomingLevel).ToString();
        }

        healthArrow.enabled = !currentExpFill.enabled;
        nextHealthLabel.enabled = !currentExpFill.enabled;
        attackArrow.enabled = !currentExpFill.enabled;
        nextAttackLabel.enabled = !currentExpFill.enabled;

        nextExpFill.enabled = scrapOffered > 0;
        nextExpFill.fillAmount = Mathf.Round(expProgress) / expRequirement;
    }

    public void OfferScrap()
    {
        scrapOffered++;

        scrapOfferedLabel.enabled = true;
        scrapOfferedLabel.text = scrapOffered.ToString();

        minusButton.interactable = true;
        minusRaycaster.enabled = true;

        UpdateUIElements();

        if (scrapOffered >= playerDataManager.LoadedData.Exp)
        {
            plusButton.interactable = false;
            plusRaycaster.enabled = false;
        }
    }

    public void MinusScrap()
    {
        scrapOffered--;

        UpdateUIElements();

        if (scrapOffered == 0)
        {
            minusButton.interactable = false;
            minusRaycaster.enabled = false;
            scrapOfferedLabel.enabled = false;
        }
        else
        {
            plusButton.interactable = true;
            plusRaycaster.enabled = true;
            scrapOfferedLabel.text = scrapOffered.ToString();
        }
    }

    public void Upgrade()
    {
        StartCoroutine(UpgradeAnimation());
    }
    
    IEnumerator UpgradeAnimation()
    {
        backButton.interactable = false;
        doneButton.interactable = false;

        int currentLevel = targetCharacter.GetLevelFromExp(maggotState.Exp);
        float currentExp = maggotState.Exp;
        float incomingExp = scrapOffered * expMultiplier;
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

            currentLevel = targetCharacter.GetLevelFromExp(xp);
            xp -= targetCharacter.GetExpRequiredForLevel(0, currentLevel);
            var requiredExp = targetCharacter.GetExpRequiredForLevel(currentLevel, currentLevel + 1);
            currentExpFill.fillAmount = xp / requiredExp;

            expBarLabel.text = (int)xp + "/" + (int)requiredExp;

            yield return null;
        }

        cardHolder.DOLocalMoveZ(-0.5f, inTime).SetEase(inEase);

        yield return new WaitForSeconds(inAndSpin);

        cardHolder.DOLocalRotate(new Vector3(0, spinAmount, 0), spinTime, RotateMode.LocalAxisAdd)
            .SetEase(spinEase);

        yield return new WaitForSeconds(spinAndOut);

        cardHolder.DOLocalMoveZ(0, outTime).SetEase(outEase);

        yield return new WaitForSeconds(spinTime);

        cardHolder.localEulerAngles = new Vector3(0, 90, 0);

        raycastMask.enabled = false;

        if (!dontConfirmUpgrade)
        {
            maggotState.Exp = targetExp;
            playerDataManager.SetExp(playerDataManager.LoadedData.Exp - scrapOffered);
            playerDataManager.SaveData();

            scrapOffered = 0;
            RefreshUI();
        }

        doneButton.interactable = true;
    }

    public void ReturnToMaggotSelect()
    {
        cardListUI.ShowUI();
        canvas.Hide();
    }

    public void FinishUpgrade()
    {
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