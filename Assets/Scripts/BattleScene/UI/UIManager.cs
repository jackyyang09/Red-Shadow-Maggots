﻿using System;
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
    //[SerializeField] TMPro.TextMeshProUGUI gameSpeedText = null;
    [SerializeField] CharacterUI bossUI = null;

    [SerializeField] TMPro.TextMeshProUGUI waveCounter = null;
    [SerializeField] TMPro.TextMeshProUGUI turnCounter = null;

    [Header("Skill System")]
    [SerializeField] SkillDetailPanel skillPanel = null;
    [SerializeField] SkillButtonUI[] skillButtons = null;
    [SerializeField] OptimizedCanvas skillTargetMessage = null;
    [SerializeField] OptimizedButton skillBackButton = null;

    [Header("System Objects")]
    [SerializeField] OptimizedCanvas winCanvas = null;
    [SerializeField] OptimizedCanvas loseCanvas = null;
    
    public bool CharacterPanelOpen { get; private set; }

    public static bool CanSelectPlayer = true;
    public static bool SelectingAllyForSkill = false;

    public static Action OnShowBattleUI;
    public static Action OnHideBattleUI;
    public static Action OnAttackCommit;
    public static Action OnEnterSkillTargetMode;
    public static Action OnExitSkillTargetMode;

    private void OnEnable()
    {
        BattleSystem.OnStartPhaseLate[BattlePhases.PlayerTurn.ToInt()] += ShowBattleUI;
        PlayerCharacter.OnSelectedPlayerCharacterChange += UpdateSkillGraphic;
        PlayerCharacter.OnPlayerQTEAttack += ShowQTEUI;

        BattleSystem.OnStartPhase[BattlePhases.BattleLose.ToInt()] += ShowLoseCanvas;
        BattleSystem.OnFinalWaveClear += ShowWinCanvas;
        SceneTweener.OnBattleTransition += UpdateWaveCounter;
        GlobalEvents.OnCharacterFinishSuperCritical += OnCharacterFinishSuperCritical;

        GameManager.OnTurnCountChanged += UpdateTurnCounter;

        OnAttackCommit += HideBattleUI;
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

        GameManager.OnTurnCountChanged -= UpdateTurnCounter;

        OnAttackCommit -= HideBattleUI;
    }

    void PlayButtonSound()
    {
        JSAM.AudioManager.PlaySound(BattleSceneSounds.UIClick);
    }

    private void OnCharacterFinishSuperCritical(BaseCharacter obj)
    {
        if (!battleSystem.FinishedTurn && battleSystem.CurrentPhase == BattlePhases.PlayerTurn)
        {
            ShowBattleUI();
        }
    }

    public void ShowBattleUI()
    {
        if (BattleSystem.Instance.CurrentPhase != BattlePhases.PlayerTurn) return;
        attackButton.Show();
        CanSelectPlayer = true;

        foreach (SkillButtonUI button in skillButtons)
        {
            button.button.Show();
            UpdateSkillGraphic(battleSystem.ActivePlayer);
        }

        OnShowBattleUI?.Invoke();
    }

    public void HideBattleUI()
    {
        attackButton.Hide();
        CanSelectPlayer = false;

        foreach (SkillButtonUI button in skillButtons)
        {
            button.button.Hide();
        }

        OnHideBattleUI?.Invoke();
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
                skillButtons[i].UpdateStatus(obj.Skills[i]);
            }
        }
    }

    public void EnterSkillTargetMode()
    {
        SelectingAllyForSkill = true;

        skillTargetMessage.Show();
        skillBackButton.Show();
        if (waveManager.IsBossWave) bossUI.HideUI();
        attackButton.Hide();

        foreach (SkillButtonUI button in skillButtons)
        {
            button.button.Hide();
        }

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

        if (waveManager.IsBossWave) bossUI.ShowUI();
        skillBackButton.Hide();
        skillTargetMessage.Hide();

        foreach (PlayerCharacter p in battleSystem.PlayerCharacters)
        {
            if (p) p.HideSelectionPointer();
        }

        battleSystem.ActivePlayer.ShowSelectionCircle();
        battleSystem.ActiveEnemy.ShowSelectionCircle();
    }

    public void ShowSkillDetails(int index)
    {
        skillPanel.UpdateDetails(battleSystem.ActivePlayer.Skills[index]);
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
        turnCounter.text = gameManager.TurnCount.ToString();
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