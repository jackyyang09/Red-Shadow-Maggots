using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RSMConstants;

public class StatRenderer : MonoBehaviour
{
    [SerializeField] StatEnum targetStat;
    [SerializeField] Color positiveModifier;
    [SerializeField] Color negativeModifier;
    [SerializeField] TextMeshProUGUI nameLabel;
    [SerializeField] TextMeshProUGUI valueLabel;

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
                break;
            case StatEnum.HealReceived:
                break;
            case StatEnum.MaxHealth:
                break;
            case StatEnum.Wait:
                stateDelegate = RenderWait;
                inBattleDelegate = RenderWait;
                break;
            case StatEnum.WaitLimit:
                stateDelegate = RenderWaitLimit;
                inBattleDelegate = RenderWaitLimit;
                break;
            case StatEnum.WaitTimer:
                break;
        }
    }

    public void UpdateStat(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        stateDelegate?.Invoke(state, character, isEnemy, valueLabel);
    }

    public void UpdateStat(BaseCharacter character)
    {
        inBattleDelegate?.Invoke(character, positiveModifier, negativeModifier, nameLabel, valueLabel);
    }

    #region Attack
    public static void RenderAttack(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.GetAttack(character.GetLevelFromExp(state.Exp)).ToString();
    }

    public static void RenderAttack(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var attackModifier = character.AttackModifier;
        valueLabel.text = Mathf.FloorToInt(character.AttackModified).ToString();
        if (character.AttackModifier > 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(pColor) + ">(+" + attackModifier + ")</color>";
        }
        else if (character.AttackModifier < 0)
        {
            valueLabel.text += " <color=#" + ColorUtility.ToHtmlStringRGBA(nColor) + ">(" + attackModifier + ")</color>";
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
        var defenseModifier = character.DefenseModifier;
        valueLabel.text = character.DefenseModified.FormatToDecimal() + "%";
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

    public static void RenderMaxHealth(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.GetMaxHealth(character.GetLevelFromExp(state.Exp), isEnemy).ToString();
    }

    public static void RenderMaxHealth(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
    }

    #region Attack Window
    public static void RenderAttackWindow(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.defenseLeniency.FormatPercentage();
    }

    public static void RenderAttackWindow(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var modifier = character.AttackLeniencyModifier.FormatToDecimal() + "%";
        valueLabel.text = character.AttackLeniencyModified.FormatToDecimal() + "%";
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

    #region Defense Window
    public static void RenderDefenseWindow(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy, TextMeshProUGUI valueLabel)
    {
        valueLabel.text = character.defenseLeniency.FormatPercentage();
    }

    public static void RenderDefenseWindow(BaseCharacter character, Color pColor, Color nColor, TextMeshProUGUI nameLabel, TextMeshProUGUI valueLabel)
    {
        var modifier = character.DefenseLeniencyModifier.FormatToDecimal() + "%";
        valueLabel.text = character.DefenseLeniencyModified.FormatToDecimal() + "%";
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
}