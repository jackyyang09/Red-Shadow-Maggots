using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using static Facade;
using MPUIKIT;

public class CharacterUI : BaseGameUI
{
    [SerializeField] protected BaseCharacter designatedCharacter;

    [Header("Crit Charge Bars")] [SerializeField]
    GridLayoutGroup layoutGroup;

    [SerializeField] List<Image> chargeBars = new List<Image>();
    [SerializeField] Color chargeEmptyColor;
    [SerializeField] Color fullColor;
    [SerializeField] private Color enemyChargeFillColor;

    [Header("Crit Charge Fill")] [SerializeField]
    float tweenTime = 0.35f;

    [SerializeField] float barFlashSpeed = 0.5f;
    [SerializeField] TextMeshProUGUI chargeText;
    [SerializeField] Image chargeFill;
    [SerializeField] private Color chargeFillColor;

    [Header("Object References")] [SerializeField]
    protected TextMeshProUGUI nameText;

    [SerializeField] protected TextMeshProUGUI levelLabel;
    [SerializeField] protected Image classIcon;
    [SerializeField] protected GameObject iconPrefab;
    [SerializeField] protected RectTransform iconContainer;
    [SerializeField] protected GameObject playerCritCanvas;
    [SerializeField] protected GameObject enemyCritCanvas;
    [SerializeField] protected MPImage waitImage;
    [SerializeField] SimpleHealth health;
    [SerializeField] ShieldUI shield;

    PlayerCharacter player;
    EnemyCharacter enemy;

    protected List<AppliedEffect> effects = new List<AppliedEffect>();
    protected List<GameEffectIconUI> icons = new List<GameEffectIconUI>();
    Dictionary<AppliedEffect, GameEffectIconUI> iconDictionary = new Dictionary<AppliedEffect, GameEffectIconUI>();

    public virtual void InitializeWithCharacter(BaseCharacter character)
    {
        designatedCharacter = character;

        classIcon.sprite = index.GetClassIcon(designatedCharacter.Reference.characterClass);
        nameText.text = designatedCharacter.Reference.characterName;
        levelLabel.text = character.CurrentLevel.ToString();
        health.InitializeWithCharacter(character);
        shield.InitializeWithCharacter(character);

        designatedCharacter.OnWaitTimeChanged += OnWaitChanged;
        designatedCharacter.OnApplyGameEffect += AddEffectIcon;
        designatedCharacter.OnEffectStacksChanged += UpdateEffectStacks;
        designatedCharacter.OnRemoveGameEffect += RemoveEffect;
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
        GlobalEvents.OnEnterBattleCutscene += HideUI;
        GlobalEvents.OnExitBattleCutscene += TryShowUI;
        UIManager.OnShowBattleUI += TryShowUIDelayed;
        UIManager.OnCharacterStatUIOpened += HideUI;
        UIManager.OnCharacterStatUIClosed += TryShowUIDelayed;
    }

    private void OnDisable()
    {
        if (designatedCharacter)
        {
            designatedCharacter.OnWaitTimeChanged -= OnWaitChanged;
            designatedCharacter.OnApplyGameEffect -= AddEffectIcon;
            designatedCharacter.OnRemoveGameEffect -= RemoveEffect;
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

        GlobalEvents.OnEnterBattleCutscene -= HideUI;
        GlobalEvents.OnExitBattleCutscene -= TryShowUI;
        UIManager.OnAttackCommit -= UpdateUIState;
        UIManager.OnShowBattleUI -= TryShowUIDelayed;
        UIManager.OnCharacterStatUIOpened -= HideUI;
        UIManager.OnCharacterStatUIClosed -= TryShowUIDelayed;
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

    void OnWaitChanged()
    {
        waitImage.fillAmount = designatedCharacter.WaitTimer;
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
        if (obj.remainingTurns == 0 && obj.remainingActivations == 0)
        {
            // Neither -1 or above 0? Likely a one-time effect
            return;
        }

        if (iconDictionary.ContainsKey(obj))
        {
            if (obj.HasStacks)
            {
                iconDictionary[obj].UpdateStackCount();
            }
        }
        else
        {
            effects.Add(obj);
            UpdateEffectIcons();
        }
    }

    private void UpdateEffectStacks(AppliedEffect effect)
    {
        iconDictionary[effect].UpdateStackCount();
    }

    private void RemoveEffect(AppliedEffect obj)
    {
        var index = effects.IndexOf(obj);
        effects.Remove(obj);
        iconDictionary.Remove(obj);
        UpdateEffectIcons(index);
    }

    void UpdateEffectIcons(int index = -1)
    {
        if (index == -1)
        {
            var newIcon = Instantiate(iconPrefab, iconContainer).GetComponent<GameEffectIconUI>();
            icons.Add(newIcon);
            newIcon.InitializeWithEffect(effects.GetLast());
            iconDictionary.Add(effects.GetLast(), newIcon);
        }
        else
        {
            Destroy(icons[index].gameObject);
            icons.RemoveAt(index);
        }
    }

    void SelfDestruct()
    {
        Destroy(gameObject);
    }

    void TryShowUI()
    {
        if (designatedCharacter)
        {
            OptimizedCanvas.Show();
        }
    }

    /// <summary>
    /// CharacterUI takes a frame to re-adjust if the camera moved significantly 
    /// such as when opening the in-battle status screen
    /// </summary>
    void TryShowUIDelayed()
    {
        StartCoroutine(TryShowUIDelayedRoutine());
    }

    IEnumerator TryShowUIDelayedRoutine()
    {
        yield return null;
        TryShowUI();
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