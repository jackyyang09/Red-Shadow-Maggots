using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class UIManager : BasicSingleton<UIManager>
{
    [SerializeField] Canvas viewPortBillboardCanvas = null;
    public Canvas ViewportBillboardCanvas { get { return viewPortBillboardCanvas; } }

    [Header("Battle System")]
    [SerializeField] OptimizedButton attackButton = null;
    [SerializeField] UICharacterDetails characterDetailsPanel = null;
    [SerializeField] QuickTimeBar offenseBar = null;
    [SerializeField] QuickTimeDefense defenseBar = null;
    [SerializeField] QuickTimeHold holdBar = null;
    [SerializeField] TMPro.TextMeshProUGUI gameSpeedText = null;
    [SerializeField] CharacterUI bossUI = null;

    [Header("Skill System")]
    [SerializeField] SkillDetailPanel skillPanel = null;
    [SerializeField] SkillButtonUI[] skillButtons = null;
    [SerializeField] OptimizedCanvas skillTargetMessage = null;
    [SerializeField] OptimizedButton skillBackButton = null;

    [Header("System Objects")]
    [SerializeField] OptimizedCanvas winCanvas = null;
    [SerializeField] OptimizedCanvas loseCanvas = null;
    [SerializeField] OptimizedCanvas optionsCanvas = null;
    [SerializeField] OptimizedButton optionsButton = null;
    [SerializeField] OptimizedButton gameSpeedButton = null;
    
    public bool CharacterPanelOpen { get; private set; }

    [SerializeField] TMPro.TextMeshProUGUI waveCounter = null;

    public static bool CanSelectPlayer = true;
    public static bool SelectingAllyForSkill = false;

    public static Action OnAttackCommit;
    public static Action OnRemovePlayerControl;
    public static Action OnResumePlayerControl;
    public static Action OnEnterSkillTargetMode;
    public static Action OnExitSkillTargetMode;

    // Start is called before the first frame update
    void Start()
    {
        CanSelectPlayer = true;
        SelectingAllyForSkill = false;
    }

    private void OnEnable()
    {
        BattleSystem.OnStartPlayerTurnLate += ResumePlayerControl;
        PlayerCharacter.OnSelectedPlayerCharacterChange += UpdateSkillGraphic;
        PlayerCharacter.OnPlayerQTEAttack += ShowQTEUI;

        BattleSystem.OnPlayerDefeat += ShowLoseCanvas;
        BattleSystem.OnFinalWaveClear += ShowWinCanvas;
        SceneTweener.OnBattleTransition += UpdateWaveCounter;
        GlobalEvents.OnModifyGameSpeed += UpdateGameSpeed;
        GlobalEvents.OnCharacterFinishSuperCritical += OnCharacterFinishSuperCritical;

        OnAttackCommit += RemovePlayerControl;
    }

    private void OnDisable()
    {
        BattleSystem.OnStartPlayerTurnLate -= ResumePlayerControl;
        PlayerCharacter.OnSelectedPlayerCharacterChange -= UpdateSkillGraphic;
        PlayerCharacter.OnPlayerQTEAttack -= ShowQTEUI;

        BattleSystem.OnPlayerDefeat -= ShowLoseCanvas;
        BattleSystem.OnFinalWaveClear -= ShowWinCanvas;
        SceneTweener.OnBattleTransition -= UpdateWaveCounter;
        GlobalEvents.OnModifyGameSpeed -= UpdateGameSpeed;
        GlobalEvents.OnCharacterFinishSuperCritical -= OnCharacterFinishSuperCritical;

        OnAttackCommit -= RemovePlayerControl;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    void PlayButtonSound()
    {
        JSAM.AudioManager.PlaySound(BattleSceneSounds.UIClick);
    }

    private void OnCharacterFinishSuperCritical(BaseCharacter obj)
    {
        if (!battleSystem.FinishedTurn && battleSystem.CurrentPhase == BattlePhases.PlayerTurn)
        {
            ResumePlayerControl();
        }
    }

    public void ResumePlayerControl()
    {
        if (BattleSystem.Instance.CurrentPhase != BattlePhases.PlayerTurn) return;
        if (optionsCanvas.IsVisible) return;
        attackButton.Show();
        CanSelectPlayer = true;

        foreach (SkillButtonUI button in skillButtons)
        {
            button.button.Show();
            UpdateSkillGraphic(battleSystem.ActivePlayer);
        }

        OnResumePlayerControl?.Invoke();
    }

    public void RemovePlayerControl()
    {
        attackButton.Hide();
        CanSelectPlayer = false;

        foreach (SkillButtonUI button in skillButtons)
        {
            button.button.Hide();
        }

        OnRemovePlayerControl?.Invoke();
    }

    public void OpenCharacterPanel()
    {
        CharacterPanelOpen = true;
        characterDetailsPanel.DisplayWithCharacter(battleSystem.ActivePlayer);
    }

    public void CloseCharacterPanel()
    {
        CharacterPanelOpen = false;
        characterDetailsPanel.Hide();
    }

    private void UpdateSkillGraphic(PlayerCharacter obj)
    {
        if (battleSystem.CurrentPhase == BattlePhases.PlayerTurn)
        {
            for (int i = 0; i < skillButtons.Length; i++)
            {
                skillButtons[i].UpdateStatus(obj.GetSkill(i));
            }
        }
    }

    public void EnterSkillTargetMode()
    {
        SelectingAllyForSkill = true;

        skillTargetMessage.Show();
        skillBackButton.Show();

        attackButton.Hide();

        foreach (SkillButtonUI button in skillButtons)
        {
            button.button.Hide();
        }

        foreach (PlayerCharacter p in BattleSystem.Instance.PlayerCharacters)
        {
            p.ShowSelectionCircle();
        }

        foreach (EnemyCharacter e in EnemyController.Instance.Enemies)
        {
            e.ForceHideSelectionPointer();
        }

        OnEnterSkillTargetMode?.Invoke();
    }

    public void CancelSkillInvocation()
    {
        battleSystem.ActivePlayer.CancelSkill();
        ResumePlayerControl();
    }

    public void ExitSkillTargetMode()
    {
        SelectingAllyForSkill = false;

        skillBackButton.Hide();
        skillTargetMessage.Hide();

        foreach (PlayerCharacter p in BattleSystem.Instance.PlayerCharacters)
        {
            p.HideSelectionPointer();
        }

        battleSystem.ActivePlayer.ShowSelectionCircle();
        battleSystem.ActiveEnemy.ShowSelectionCircle();
    }

    public void ShowSkillDetails(int index)
    {
        skillPanel.UpdateDetails(battleSystem.ActivePlayer.GetSkill(index));
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

    public void ShowSettingsMenu()
    {
        optionsCanvas.Show();
        PlayButtonSound();
        RemovePlayerControl();
        optionsButton.Hide();
        gameSpeedButton.Hide();
    }

    public void HideSettingsMenu()
    {
        optionsCanvas.Hide();
        PlayButtonSound();
        ResumePlayerControl();
        optionsButton.Show();
        gameSpeedButton.Show();
    }

    private void UpdateWaveCounter()
    {
        waveCounter.text = (EnemyWaveManager.Instance.CurrentWaveCount + 1) + "/" + EnemyWaveManager.Instance.TotalWaves;
    }

    public void UpdateGameSpeed()
    {
        gameSpeedText.text = BattleSystem.Instance.CurrentGameSpeedTime + "x";
    }

    public void InitializeBossUIWithCharacter(BaseCharacter character)
    {
        bossUI.InitializeWithCharacter(character);
        bossUI.OptimizedCanvas.Show();
    }
}   