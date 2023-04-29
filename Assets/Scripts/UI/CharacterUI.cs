﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using static Facade;

public class CharacterUI : BaseGameUI
{
    [SerializeField] protected BaseCharacter designatedCharacter = null;
    [SerializeField] protected List<AppliedEffect> effects = new List<AppliedEffect>();
    List<int> indicesToRemove = new List<int>();

    [Header("Crit Charge Bars")] [SerializeField]
    GridLayoutGroup layoutGroup = null;

    [SerializeField] List<Image> chargeBars = new List<Image>();
    [SerializeField] Color chargeEmptyColor;
    [SerializeField] Color fullColor;
    [SerializeField] private Color enemyChargeFillColor;

    [Header("Crit Charge Fill")] [SerializeField]
    float tweenTime = 0.35f;

    [SerializeField] float barFlashSpeed = 0.5f;
    [SerializeField] TextMeshProUGUI chargeText = null;
    [SerializeField] Image chargeFill = null;
    [SerializeField] private Color chargeFillColor;

    [Header("Object References")] [SerializeField]
    protected TextMeshProUGUI nameText = null;

    [SerializeField] protected TextMeshProUGUI levelLabel = null;
    [SerializeField] protected Image classIcon = null;
    [SerializeField] protected GameObject iconPrefab = null;
    [SerializeField] protected RectTransform iconContainer = null;
    [SerializeField] protected GameObject playerCritCanvas = null;
    [SerializeField] protected GameObject enemyCritCanvas = null;
    protected List<Image> iconImages = new List<Image>();
    [SerializeField] SimpleHealth health = null;

    PlayerCharacter player;
    EnemyCharacter enemy;

    public virtual void InitializeWithCharacter(BaseCharacter character)
    {
        designatedCharacter = character;

        classIcon.sprite = index.GetClassIcon(designatedCharacter.Reference.characterClass);
        nameText.text = designatedCharacter.Reference.characterName;
        levelLabel.text = character.CurrentLevel.ToString();
        health.InitializeWithCharacter(character);

        designatedCharacter.onApplyGameEffect += AddEffectIcon;
        designatedCharacter.onRemoveGameEffect += QueueEffectIconRemoval;
        designatedCharacter.onDeath.AddListener(SelfDestruct);

        enemy = designatedCharacter as EnemyCharacter;
        if (enemy)
        {
            enemy.onCritLevelChanged += UpdateEnemyCritCharge;

            for (int i = chargeBars.Count - 1; i >= enemy.Reference.turnsToCrit; i--)
            {
                chargeBars[i].gameObject.SetActive(false);
                chargeBars.RemoveAt(i);
                UpdateEnemyCritCharge(0);
            }
        }

        if (playerCritCanvas)
        {
            playerCritCanvas.SetActive(!enemy);
        }

        if (enemyCritCanvas)
        {
            enemyCritCanvas.SetActive(enemy);
        }

        if (!enemy)
        {
            player = designatedCharacter as PlayerCharacter;
            player.OnCharacterCritChanceChanged += UpdatePlayerCritCharge;
            chargeFill.fillAmount = 0;
            UpdatePlayerCritCharge();
        }
    }

    private void OnEnable()
    {
        GlobalEvents.OnEnterBattleCutscene += OptimizedCanvas.Hide;
        GlobalEvents.OnExitBattleCutscene += OptimizedCanvas.Show;
        UIManager.OnShowBattleUI += OptimizedCanvas.Show;
        BattleSystem.OnFinishTickingEffects += RemoveEffects;
    }

    private void OnDisable()
    {
        if (designatedCharacter)
        {
            designatedCharacter.onApplyGameEffect -= AddEffectIcon;
            designatedCharacter.onRemoveGameEffect -= QueueEffectIconRemoval;
            designatedCharacter.onDeath.RemoveListener(SelfDestruct);
        }

        if (enemy)
        {
            enemy.onCritLevelChanged -= UpdateEnemyCritCharge;
        }

        if (player)
        {
            player.OnCharacterCritChanceChanged -= UpdatePlayerCritCharge;
        }

        GlobalEvents.OnEnterBattleCutscene -= OptimizedCanvas.Hide;
        GlobalEvents.OnExitBattleCutscene -= OptimizedCanvas.Show;
        UIManager.OnAttackCommit -= UpdateUIState;
        UIManager.OnShowBattleUI -= OptimizedCanvas.Show;
        BattleSystem.OnFinishTickingEffects -= RemoveEffects;
    }

    private void Update()
    {
        if (!player) return;
        if (chargeFill.fillAmount == 1)
        {
            chargeFill.color = new Color(chargeFillColor.r, chargeFillColor.g, chargeFillColor.b,
                Mathf.PingPong(Time.time * barFlashSpeed, 1));
        }
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

    private void UpdateEnemyCritCharge(int chargeLevel)
    {
        if (!enemy.CanCrit)
        {
            for (int i = 0; i < chargeLevel; i++)
            {
                chargeBars[i].color = enemyChargeFillColor;
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

    private void UpdatePlayerCritCharge()
    {
        chargeFill.DOKill();
        if (chargeFill.fillAmount < 1) chargeFill.color = chargeFillColor;
        chargeFill.DOFillAmount(designatedCharacter.CritChanceModified, tweenTime).OnUpdate(() =>
        {
            chargeText.text = Mathf.RoundToInt(chargeFill.fillAmount * 100).ToString() + "%";
        }).OnComplete(() =>
        {
            //if (designatedCharacter.CritChanceModified >= 1)
            //{
            //    chargeFill.color = Color.white;
            //    chargeFill.DOFade(0, barFlashTime).SetEase(Ease.Flash, 2).SetLoops(-1);
            //}
        });
    }

    private void AddEffectIcon(AppliedEffect obj)
    {
        effects.Add(obj);
        UpdateEffectIcons();
    }

    private void QueueEffectIconRemoval(AppliedEffect obj)
    {
        var index = effects.IndexOf(obj);
        UpdateEffectIcons(index);
        effects.Remove(obj);
    }

    void UpdateEffectIcons(int index = -1)
    {
        if (index == -1)
        {
            var newIcon = Instantiate(iconPrefab, iconContainer).GetComponent<Image>();
            iconImages.Add(newIcon);
            newIcon.sprite = effects[effects.Count - 1].referenceEffect.effectIcon;
        }
        else
        {
            indicesToRemove.Add(index);
        }
    }

    private void RemoveEffects()
    {
        for (int i = indicesToRemove.Count - 1; i > -1; i--)
        {
            Destroy(iconImages[indicesToRemove[i]].gameObject);
            iconImages.RemoveAt(indicesToRemove[i]);
        }
    }

    void SelfDestruct()
    {
        Destroy(gameObject);
    }

    public override void ShowUI()
    {
        optimizedCanvas.Show();
    }

    public override void HideUI()
    {
        optimizedCanvas.Hide();
    }
}