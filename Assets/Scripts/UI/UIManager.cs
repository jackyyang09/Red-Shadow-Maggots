using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Battle System")]
    [SerializeField] OptimizedButton attackButton = null;
    [SerializeField] UICharacterDetails characterDetailsPanel = null;
    [SerializeField] QuickTimeBar offenseBar = null;
    [SerializeField] QuickTimeBar defenseBar = null;
    [SerializeField] QuickTimeHold holdBar = null;
    [SerializeField] TMPro.TextMeshProUGUI gameSpeedText = null;

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

    public static Action onAttackCommit;
    public static UIManager instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    void Start()
    {
        CanSelectPlayer = true;
        SelectingAllyForSkill = false;
    }

    private void OnEnable()
    {
        BattleSystem.onStartPlayerTurnLate += ResumePlayerControl;
        PlayerCharacter.onSelectedPlayerCharacterChange += UpdateSkillGraphic;

        GlobalEvents.OnPlayerDefeat += ShowLoseCanvas;
        GlobalEvents.OnFinalWaveClear += ShowWinCanvas;
        GlobalEvents.OnEnterWave += UpdateWaveCounter;
        GlobalEvents.OnModifyGameSpeed += UpdateGameSpeed;

        onAttackCommit += RemovePlayerControl;
    }

    private void OnDisable()
    {
        BattleSystem.onStartPlayerTurnLate -= ResumePlayerControl;
        PlayerCharacter.onSelectedPlayerCharacterChange -= UpdateSkillGraphic;

        GlobalEvents.OnPlayerDefeat -= ShowLoseCanvas;
        GlobalEvents.OnFinalWaveClear -= ShowWinCanvas;
        GlobalEvents.OnEnterWave -= UpdateWaveCounter;
        GlobalEvents.OnModifyGameSpeed -= UpdateGameSpeed;

        onAttackCommit -= RemovePlayerControl;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    void PlayButtonSound()
    {
        JSAM.AudioManager.PlaySound(JSAM.Sounds.UIClick);
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
            UpdateSkillGraphic(BattleSystem.Instance.GetActivePlayer());
        }
    }

    public void RemovePlayerControl()
    {
        attackButton.Hide();
        CanSelectPlayer = false;

        foreach (SkillButtonUI button in skillButtons)
        {
            button.button.Hide();
        }
    }

    public void OpenCharacterPanel()
    {
        CharacterPanelOpen = true;
        characterDetailsPanel.DisplayWithCharacter(BattleSystem.Instance.GetActivePlayer());
    }

    public void CloseCharacterPanel()
    {
        CharacterPanelOpen = false;
        characterDetailsPanel.Hide();
    }

    private void UpdateSkillGraphic(PlayerCharacter obj)
    {
        if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn)
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
            p.ShowSelectionPointer();
        }

        foreach (EnemyCharacter e in EnemyController.instance.enemies)
        {
            e.ForceHideSelectionPointer();
        }
    }

    public void CancelSkillInvocation()
    {
        BattleSystem.Instance.GetActivePlayer().CancelSkill();
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

        BattleSystem.Instance.GetActiveEnemy().ShowSelectionPointer();
    }

    public void ShowSkillDetails(int index)
    {
        skillPanel.UpdateDetails(BattleSystem.Instance.GetActivePlayer().GetSkill(index));
    }

    public void AttackPress()
    {
        onAttackCommit?.Invoke();
        BattleSystem.Instance.ExecutePlayerAttack();

        PlayerCharacter player = BattleSystem.Instance.GetActivePlayer();
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
        defenseBar.InitializeBar(BattleSystem.Instance.GetActivePlayer());
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
        waveCounter.text = (EnemyWaveManager.instance.CurrentWave + 1) + "/" + EnemyWaveManager.instance.TotalWaves;
    }

    public void UpdateGameSpeed()
    {
        gameSpeedText.text = BattleSystem.Instance.CurrentGameSpeedTime + "x";
    }

    void EstablishSingletonDominance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // A unique case where the Singleton exists but not in this scene
            if (instance.gameObject.scene.name == null)
            {
                instance = this;
            }
            else if (!instance.gameObject.activeInHierarchy)
            {
                instance = this;
            }
            else if (instance.gameObject.scene.name != gameObject.scene.name)
            {
                instance = this;
            }
            Destroy(gameObject);
        }
    }
}   