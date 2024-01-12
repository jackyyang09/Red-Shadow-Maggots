using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RSMConstants;

public class StatRenderer : MonoBehaviour
{
    [SerializeField] protected StatEnum targetStat;
    [SerializeField] protected Color positiveModifier;
    [SerializeField] protected Color negativeModifier;
    [SerializeField] protected TextMeshProUGUI nameLabel;
    [SerializeField] protected TextMeshProUGUI valueLabel;

    delegate void StateDelegate(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel);
    StateDelegate stateDelegate;

    delegate void InBattleDelegate(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel);
    InBattleDelegate inBattleDelegate;

    // Start is called before the first frame update
    void Start()
    {
        switch (targetStat)
        {
            case StatEnum.Attack:
                stateDelegate = RenderAttack;
                inBattleDelegate = RenderAttack;
                break;
            case StatEnum.AttackWindow:
                stateDelegate = RenderAttackWindow;
                inBattleDelegate = RenderAttackWindow;
                break;
            case StatEnum.CurrentHealth:
                break;
            case StatEnum.CritChance:
                stateDelegate = RenderCritRate;
                inBattleDelegate = RenderCritRate;
                break;
            case StatEnum.CritDamage:
                stateDelegate = RenderCritDamage;
                inBattleDelegate = RenderCritDamage;
                break;
            case StatEnum.Defense:
                stateDelegate = RenderDefense;
                inBattleDelegate = RenderDefense;
                break;
            case StatEnum.DefenseWindow:
                stateDelegate = RenderDefenseWindow;
                inBattleDelegate = RenderDefenseWindow;
                break;
            case StatEnum.Health:
                inBattleDelegate = RenderHealth;
                break;
            case StatEnum.HealReceived:
                break;
            case StatEnum.Level:
                stateDelegate = RenderLevel;
                inBattleDelegate = RenderLevel;
                break;
            case StatEnum.MaxHealth:
                stateDelegate = RenderMaxHealth;
                inBattleDelegate = RenderMaxHealth;
                break;
            case StatEnum.Name:
                stateDelegate = RenderName;
                inBattleDelegate = RenderName;
                break;
            case StatEnum.Shield:
                inBattleDelegate = RenderShield;
                break;
            case StatEnum.Wait:
                stateDelegate = RenderWait;
                inBattleDelegate = RenderWait;
                break;
            case StatEnum.WaitFull:
                inBattleDelegate = RenderWaitFull;
                break;
            case StatEnum.WaitLimit:
                stateDelegate = RenderWaitLimit;
                inBattleDelegate = RenderWaitLimit;
                break;
            case StatEnum.WaitPercent:
                inBattleDelegate = RenderWaitPercent;
                break;
            case StatEnum.WaitTimer:
                break;
        }
    }

    public virtual void UpdateStat(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        stateDelegate?.Invoke(state, character, isEnemy, valueLabel);
    }

    public virtual void UpdateStat(BaseCharacter character)
    {
        inBattleDelegate?.Invoke(character, positiveModifier, negativeModifier, nameLabel, valueLabel);
    }

    /// <summary>
    /// Changing the description label takes too long and ruins layout. 
    /// A rebuild is necessary at this stage
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayedRebuild()
    {
        yield return null;

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
    }

    [ContextMenu(nameof(Test))]
    void Test()
    {
        UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(valueLabel.transform.parent.GetRectTransform());
    }

    [ContextMenu(nameof(Test2))]
    void Test2()
    {
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(valueLabel.transform.parent.GetRectTransform());
    }

    #region Attack
    public static void RenderAttack(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.GetAttack(character.GetLevelFromExp(state.Exp)).ToString();
    }

    public static void RenderAttack(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var attackModifier = Mathf.FloorToInt(character.AttackModifier);
        valueLabel.text = Mathf.FloorToInt(character.AttackModified).ToString();
        if (attackModifier > 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(pColor) + ">(+" + attackModifier + ")</color>";
        }
        else if (attackModifier < 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(nColor) + ">(" + attackModifier + ")</color>";
        }
    }
    #endregion

    #region Attack Window
    public static void RenderAttackWindow(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.defenseLeniency.FormatPercentage();
    }

    public static void RenderAttackWindow(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var modifier = character.AttackLeniencyModifier.FormatPercentage();
        valueLabel.text = character.AttackLeniencyModified.FormatPercentage();
        if (character.AttackLeniencyModifier > 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(pColor) + ">(+" + modifier + ")</color>";
        }
        else if (character.AttackLeniencyModifier < 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(nColor) + ">(" + modifier + ")</color>";
        }
    }
    #endregion

    #region Defense
    public static void RenderDefense(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.GetDefense(character.GetLevelFromExp(state.Exp)).FormatPercentage();
    }

    public static void RenderDefense(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var defenseModifier = character.DefenseModifier.FormatPercentage();
        valueLabel.text = character.DefenseModified.FormatPercentage();
        if (character.DefenseModifier > 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(pColor) + ">(+" + defenseModifier + ")</color>";
        }
        else if (character.DefenseModifier < 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(nColor) + ">(" + defenseModifier + ")</color>";
        }
    }
    #endregion

    #region Defense Window
    public static void RenderDefenseWindow(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.defenseLeniency.FormatPercentage();
    }

    public static void RenderDefenseWindow(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var modifier = character.DefenseLeniencyModifier.FormatPercentage();
        valueLabel.text = character.DefenseLeniencyModified.FormatPercentage();
        if (character.DefenseLeniencyModifier > 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(pColor) + ">(+" + modifier + ")</color>";
        }
        else if (character.DefenseLeniencyModifier < 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(nColor) + ">(" + modifier + ")</color>";
        }
    }
    #endregion

    #region Crit Damage
    public static void RenderCritDamage(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.critDamageMultiplier.FormatPercentage();
    }

    public static void RenderCritDamage(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var modifiedCritDamage = character.CritDamageModifier.FormatPercentage();
        valueLabel.text = character.CritDamageModified.FormatPercentage();
        if (character.CritDamageModifier > 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(pColor) + ">(+" + modifiedCritDamage + ")</color>";
        }
        else if (character.CritDamageModifier < 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(nColor) + ">(" + modifiedCritDamage + ")</color>";
        }
    }
    #endregion

    #region Crit Rate
    public static void RenderCritRate(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.critChance.FormatPercentage();
    }

    public static void RenderCritRate(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var modifiedCritChance = character.CritChanceModifier.FormatPercentage();
        valueLabel.text = character.CritChanceModified.FormatPercentage();
        if (character.CritChanceModifier > 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(pColor) + ">(+" + modifiedCritChance + ")</color>";
        }
        else if (character.CritChanceModifier < 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(nColor) + ">(" + modifiedCritChance + ")</color>";
        }
    }
    #endregion

    #region Health
    public static void RenderHealth(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = Mathf.CeilToInt(character.CurrentHealth) + "/" + character.MaxHealth;
    }
    #endregion

    #region Level
    public static void RenderLevel(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = "Lvl. " + character.GetLevelFromExp(state.Exp);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(valueLabel.transform.parent.GetRectTransform());
    }

    public static void RenderLevel(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = "Lvl. " + character.CurrentLevel;
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(valueLabel.transform.parent.GetRectTransform());
    }
    #endregion

    #region Max Health
    public static void RenderMaxHealth(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.GetMaxHealth(character.GetLevelFromExp(state.Exp), isEnemy).ToString();
    }

    public static void RenderMaxHealth(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.MaxHealth.ToString();
    }
    #endregion

    #region Name
    public static void RenderName(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.characterName;
        UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(valueLabel.transform.parent.GetRectTransform());
    }

    public static void RenderName(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.Reference.characterName;
        UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(valueLabel.transform.parent.GetRectTransform());
    }
    #endregion

    #region Shield
    public static void RenderShield(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        //valueLabel.text = character.GetMaxHealth(character.GetLevelFromExp(state.Exp), isEnemy).ToString();
    }

    public static void RenderShield(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = Mathf.FloorToInt(character.CurrentShield).ToString();
    }
    #endregion

    #region Wait
    public static void RenderWait(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.wait.FormatToDecimal();
    }

    public static void RenderWait(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var modifier = character.WaitModifier.FormatToDecimal();
        valueLabel.text = character.WaitModified.FormatToDecimal();
        if (character.WaitModifier > 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(nColor) + ">(+" + modifier + ")</color>";
        }
        else if (character.WaitModifier < 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(pColor) + ">(" + modifier + ")</color>";
        }
    }
    #endregion

    #region Wait Full
    public static void RenderWaitFull(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.Wait.FormatToDecimal() + "/" + character.WaitLimitModified.FormatToDecimal();
    }
    #endregion

    #region Wait Limit
    public static void RenderWaitLimit(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.waitLimit.FormatToDecimal();
    }

    public static void RenderWaitLimit(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var modifier = character.WaitLimitModifier.FormatToDecimal();
        valueLabel.text = character.WaitLimitModified.FormatToDecimal();
        if (character.WaitLimitModifier > 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(pColor) + ">(+" + modifier + ")</color>";
        }
        else if (character.WaitLimitModifier < 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(nColor) + ">(" + modifier + ")</color>";
        }
    }
    #endregion

    #region Wait Percent
    public static void RenderWaitPercent(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.WaitPercentage.FormatToDecimal() + "%";
    }
    #endregion
}