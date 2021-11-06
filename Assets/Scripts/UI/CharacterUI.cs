using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Facade;

public class CharacterUI : BaseGameUI
{
    [SerializeField] protected BaseCharacter designatedCharacter = null;
    [SerializeField] protected List<BaseGameEffect> effects = new List<BaseGameEffect>();

    [Header("Crit Charge Bars")]
    [SerializeField] GridLayoutGroup layoutGroup = null;
    [SerializeField] List<Image> chargeBars = new List<Image>();
    [SerializeField] Color chargeEmptyColor;
    [SerializeField] Color fullColor;

    [Header("Object References")]
    [SerializeField] protected TextMeshProUGUI nameText = null;
    [SerializeField] protected Image classIcon = null;
    [SerializeField] protected GameObject iconPrefab = null;
    [SerializeField] protected RectTransform iconContainer = null;
    [SerializeField] protected GameObject critCanvas = null;
    protected List<Image> iconImages = new List<Image>();
    [SerializeField] SimpleHealth health = null;

    EnemyCharacter enemy;

    public virtual void InitializeWithCharacter(BaseCharacter character)
    {
        designatedCharacter = character;

        classIcon.sprite = index.GetClassIcon(designatedCharacter.Reference.characterClass);
        nameText.text = designatedCharacter.Reference.characterName;
        health.InitializeWithCharacter(character);

        designatedCharacter.onApplyGameEffect += AddEffectIcon;
        designatedCharacter.onRemoveGameEffect += RemoveEffectIcon;
        designatedCharacter.onDeath.AddListener(SelfDestruct);

        enemy = designatedCharacter as EnemyCharacter;
        if (enemy)
        {
            enemy.onCritLevelChanged += UpdateCritChargeLevel;
            critCanvas.SetActive(true);

            for (int i = chargeBars.Count - 1; i >= enemy.Reference.turnsToCrit; i--)
            {
                chargeBars[i].gameObject.SetActive(false);
                chargeBars.RemoveAt(i);
                UpdateCritChargeLevel(0);
            }
        }
        else
        {
            critCanvas.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        GlobalEvents.OnEnterBattleCutscene += OptimizedCanvas.Hide;
        GlobalEvents.OnExitBattleCutscene += OptimizedCanvas.Show;
        //UIManager.OnAttackCommit += UpdateUIState;
        UIManager.OnResumePlayerControl += OptimizedCanvas.Show;
        //BattleSystem.OnStartEnemyTurnLate += UpdateUIState;
    }

    private void OnDisable()
    {
        if (designatedCharacter)
        {
            designatedCharacter.onApplyGameEffect -= AddEffectIcon;
            designatedCharacter.onRemoveGameEffect -= RemoveEffectIcon;
            designatedCharacter.onDeath.RemoveListener(SelfDestruct);
        }

        if (enemy)
        {
            enemy.onCritLevelChanged -= UpdateCritChargeLevel;
        }

        GlobalEvents.OnEnterBattleCutscene -= OptimizedCanvas.Hide;
        GlobalEvents.OnExitBattleCutscene -= OptimizedCanvas.Show;
        UIManager.OnAttackCommit -= UpdateUIState;
        UIManager.OnResumePlayerControl -= OptimizedCanvas.Show;
        BattleSystem.OnStartEnemyTurnLate -= UpdateUIState;
    }

    private void UpdateUIState()
    {
        if ((battleSystem.ActiveEnemy == enemy && enemy != null) || battleSystem.ActivePlayer == designatedCharacter)
        {
            OptimizedCanvas.Show();
        }
        else
        {
            OptimizedCanvas.Hide();
        }
    }

    private void UpdateCritChargeLevel(int chargeLevel)
    {
        if (!enemy.CanCrit)
        {
            for (int i = 0; i < chargeLevel; i++)
            {
                chargeBars[i].color = Color.white;
            }

            for (int i = chargeLevel; i < chargeBars.Count; i++)
            {
                chargeBars[i].color = chargeEmptyColor;
            }
        }
        else
        {
            for (int i = 0; i < chargeBars.Count; i++)
            {
                chargeBars[i].color = fullColor;
            }
        }
    }

    private void AddEffectIcon(BaseGameEffect obj)
    {
        effects.Add(obj);
        UpdateEffectIcons();
        OptimizedCanvas.FlashLayoutComponents();
    }

    private void RemoveEffectIcon(BaseGameEffect obj)
    {
        UpdateEffectIcons(effects.IndexOf(obj));
        effects.Remove(obj);
    }

    void UpdateEffectIcons(int index = -1)
    {
        if (index == -1)
        {
            var newIcon = Instantiate(iconPrefab, iconContainer).GetComponent<Image>();
            iconImages.Add(newIcon);
            newIcon.sprite = effects[effects.Count - 1].effectIcon;
        }
        else
        {
            Destroy(iconImages[index].gameObject);
            iconImages.RemoveAt(index);
            for (int i = index; i < iconImages.Count; i++)
            {
                var icon = iconImages[i].transform as RectTransform;
            }
        }
    }

    void SelfDestruct()
    {
        Destroy(gameObject);
    }
}