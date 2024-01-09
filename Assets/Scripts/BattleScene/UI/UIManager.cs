using System;
using System.Linq;
using UnityEngine;
using static Facade;

public class UIManager : BasicSingleton<UIManager>
{
    [SerializeField] Canvas viewPortBillboardCanvas;

    public Canvas ViewportBillboardCanvas
    {
        get { return viewPortBillboardCanvas; }
    }

    [Header("Battle System")]
    [SerializeField] OptimizedButton attackButton;

    [SerializeField] QuickTimeBar offenseBar;
    public QuickTimeBar OffenseBar => offenseBar;
    [SerializeField] QuickTimeDefense defenseBar;
    [SerializeField] QuickTimeHold holdBar;

    //[SerializeField] TMPro.TextMeshProUGUI gameSpeedText;
    [SerializeField] CharacterUI bossUI;

    [SerializeField] CanvasGroup waveTurnCG;
    [SerializeField] TMPro.TextMeshProUGUI waveCounter;
    [SerializeField] TMPro.TextMeshProUGUI turnCounter;

    [Header("Skill System")] [SerializeField]
    SkillDetailPanel skillPanel;

    //[SerializeField] SkillButtonUI[] skillButtons;
    [SerializeField] OptimizedCanvas skillTargetMessage;
    [SerializeField] OptimizedButton skillBackButton;

    [SerializeField] private SkillManagerUI _skillManagerUI;

    [Header("System Objects")]
    [SerializeField] OptimizedButton settingsButton;
    [SerializeField] OptimizedCanvas winCanvas;
    [SerializeField] OptimizedCanvas loseCanvas;

    public bool CharacterPanelOpen { get; private set; }

    public static bool CanSelectCharacter = true;
    /// <summary>
    /// Can't we use a UI ray-cast blocker for this?
    /// </summary>
    public static bool SelectingAllyForSkill = false;

    public static Action OnShowBattleUI;
    public static Action OnHideBattleUI;
    public static Action OnAttackCommit;
    public static Action OnEnterSkillTargetMode;
    public static Action OnExitSkillTargetMode;
    public static Action OnCharacterStatUIOpened;
    public static Action OnCharacterStatUIClosed;

    private void OnEnable()
    {
        BattleSystem.OnStartPhaseLate[BattlePhases.PlayerTurn.ToInt()] += ShowBattleUI;
        PlayerCharacter.OnSelectedPlayerCharacterChange += UpdateSkillGraphic;
        PlayerCharacter.OnPlayerQTEAttack += ShowQTEUI;

        BattleSystem.OnStartPhase[BattlePhases.BattleLose.ToInt()] += ShowLoseCanvas;
        BattleSystem.OnFinalWaveClear += ShowWinCanvas;
        SceneTweener.OnBattleTransition += UpdateWaveCounter;
        GlobalEvents.OnCharacterFinishSuperCritical += OnCharacterFinishSuperCritical;

        GameManager.OnRoundCountChanged += UpdateTurnCounter;

        CharacterPreviewUI.Instance.OnHide += OnCloseCharacterPanel;

        OnAttackCommit += HideBattleUI;

        _skillManagerUI.Initialize(5);
        _skillManagerUI.ShowDetails += ShowSkillDetails;
        _skillManagerUI.HideDetails += HideSkillDetails;
    }

    private void OnDisable()
    {
        BattleSystem.OnStartPhaseLate[BattlePhases.PlayerTurn.ToInt()] -= ShowBattleUI;
        PlayerCharacter.OnSelectedPlayerCharacterChange -= UpdateSkillGraphic;
        PlayerCharacter.OnPlayerQTEAttack -= ShowQTEUI;

        BattleSystem.OnStartPhase[BattlePhases.BattleLose.ToInt()] -= ShowLoseCanvas;
        BattleSystem.OnFinalWaveClear -= ShowWinCanvas;
        SceneTweener.OnBattleTransition -= UpdateWaveCounter;
        GlobalEvents.OnCharacterFinishSuperCritical -= OnCharacterFinishSuperCritical;

        GameManager.OnRoundCountChanged -= UpdateTurnCounter;

        CharacterPreviewUI.Instance.OnHide -= OnCloseCharacterPanel;

        OnAttackCommit -= HideBattleUI;

        _skillManagerUI.ShowDetails -= ShowSkillDetails;
        _skillManagerUI.HideDetails -= HideSkillDetails;
    }

    private void OnCharacterFinishSuperCritical(BaseCharacter obj)
    {
        switch (battleSystem.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                if (!battleSystem.FinishedTurn)
                {
                    ShowBattleUI();
                }
                break;
            case BattlePhases.EnemyTurn:
                break;
        }
    }

    public void ShowBattleUI()
    {
        if (BattleSystem.Instance.CurrentPhase != BattlePhases.PlayerTurn) return;
        attackButton.Show();
        CanSelectCharacter = true;

        UpdateSkillGraphic(battleSystem.ActivePlayer);

        OnShowBattleUI?.Invoke();
    }

    public void HideBattleUI()
    {
        attackButton.Hide();
        CanSelectCharacter = false;

        _skillManagerUI.HideAllButtons();

        OnHideBattleUI?.Invoke();
    }

    public void OpenCharacterPanel(BaseCharacter character)
    {
        CharacterPanelOpen = true;
        //UICharacterDetails.Instance.DisplayWithCharacter(character);
        CharacterPreviewUI.Instance.DisplayWithCharacter(character);
        UICharacterDetails.Instance.DisplayWithCharacter(character);
        HideBattleUI();
        settingsButton.Hide();
        waveTurnCG.alpha = 0;
        OnCharacterStatUIOpened?.Invoke();
    }

    public void OnCloseCharacterPanel()
    {
        CharacterPanelOpen = false;
        ShowBattleUI();
        settingsButton.Show();
        waveTurnCG.alpha = 1;
        OnCharacterStatUIClosed?.Invoke();
    }

    private void UpdateSkillGraphic(PlayerCharacter obj)
    {
        if (battleSystem.CurrentPhase == BattlePhases.PlayerTurn)
        {
            _skillManagerUI.SetSkills(obj.Skills);
        }
    }

    public void EnterSkillTargetMode()
    {
        SelectingAllyForSkill = true;

        skillTargetMessage.Show();
        skillBackButton.Show();
        if (enemyController.EnemyList.Any(e => e.Reference.isBoss)) bossUI.HideUI();
        attackButton.Hide();

        _skillManagerUI.HideAllButtons();

        foreach (PlayerCharacter p in battleSystem.PlayerCharacters)
        {
            if (p) p.ShowSelectionCircle();
        }

        foreach (EnemyCharacter e in EnemyController.Instance.Enemies)
        {
            if (e) e.ForceHideSelectionPointer();
        }

        OnEnterSkillTargetMode?.Invoke();
    }

    public void CancelSkillInvocation()
    {
        battleSystem.ActivePlayer.CancelSkill();
        ShowBattleUI();
    }

    public void ExitSkillTargetMode()
    {
        SelectingAllyForSkill = false;

        if (enemyController.EnemyList.Any(e => e.Reference.isBoss)) bossUI.ShowUI();
        skillBackButton.Hide();
        skillTargetMessage.Hide();

        foreach (PlayerCharacter p in battleSystem.PlayerCharacters)
        {
            if (p) p.HideSelectionPointer();
        }

        battleSystem.ActivePlayer.ShowSelectionCircle();
        battleSystem.ActiveEnemy.ShowSelectionCircle();
    }

    private void ShowSkillDetails(GameSkill skill)
    {
        var selectedSkill = battleSystem.ActivePlayer.Skills.FirstOrDefault(x => x == skill);
        if (selectedSkill == null) return;

        skillPanel.ShowWithDetails(selectedSkill);
    }

    private void HideSkillDetails(GameSkill skill)
    {
        skillPanel.HidePanel();
    }

    public void AttackPress()
    {
        BattleSystem.Instance.BeginPlayerAttack();
        OnAttackCommit?.Invoke();
    }

    public void ShowQTEUI(PlayerCharacter player)
    {
        switch (player.Reference.attackQteType)
        {
            case QTEType.SimpleBar:
                offenseBar.InitializeBar(player);
                break;
            case QTEType.Hold:
                holdBar.InitializeBar(player);
                break;
        }
    }

    public void StartDefending()
    {
        defenseBar.InitializeDefenseBar();
    }

    public void ShowWinCanvas()
    {
        winCanvas.SetActive(true);
    }

    public void ShowLoseCanvas()
    {
        loseCanvas.SetActive(true);
    }

    private void UpdateWaveCounter()
    {
        waveCounter.text = (waveManager.WaveCount + 1) + "/" + waveManager.TotalWaves;
    }

    private void UpdateTurnCounter()
    {
        turnCounter.text = gameManager.RoundCount.ToString();
    }

    public void InitializeBossUIWithCharacter(BaseCharacter character)
    {
        bossUI.InitializeWithCharacter(character);
        bossUI.OptimizedCanvas.Show();
    }

    /// <summary>
    /// Called by the Boss Character when they spawn with 0 hp
    /// </summary>
    public void DestroyBossUI()
    {
        Destroy(bossUI.gameObject);
    }

    //public void UpdateGameSpeed()
    //{
    //    gameSpeedText.text = BattleSystem.Instance.CurrentGameSpeedTime + "x";
    //}
}