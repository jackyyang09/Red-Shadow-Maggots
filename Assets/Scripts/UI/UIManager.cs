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

    public static bool CanSelect = true;
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
        CanSelect = true;
        SelectingAllyForSkill = false;
    }

    private void OnEnable()
    {
        BattleSystem.onStartPlayerTurnLate += ResumePlayerControl;
        PlayerCharacter.onSelectedPlayerCharacterChange += UpdateSkillGraphic;

        GlobalEvents.onPlayerDefeat += ShowLoseCanvas;
        GlobalEvents.onFinalWaveClear += ShowWinCanvas;
        GlobalEvents.onEnterWave += UpdateWaveCounter;
        GlobalEvents.onModifyGameSpeed += UpdateGameSpeed;

        onAttackCommit += RemovePlayerControl;
    }

    private void OnDisable()
    {
        BattleSystem.onStartPlayerTurnLate -= ResumePlayerControl;
        PlayerCharacter.onSelectedPlayerCharacterChange -= UpdateSkillGraphic;

        GlobalEvents.onPlayerDefeat -= ShowLoseCanvas;
        GlobalEvents.onFinalWaveClear -= ShowWinCanvas;
        GlobalEvents.onEnterWave -= UpdateWaveCounter;
        GlobalEvents.onModifyGameSpeed -= UpdateGameSpeed;

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
        attackButton.Show();
        CanSelect = true;

        foreach (SkillButtonUI button in skillButtons)
        {
            button.button.Show();
            UpdateSkillGraphic(BattleSystem.instance.GetActivePlayer());
        }
    }

    public void RemovePlayerControl()
    {
        attackButton.Hide();
        CanSelect = false;

        foreach (SkillButtonUI button in skillButtons)
        {
            button.button.Hide();
        }
    }

    public void OpenCharacterPanel()
    {
        CharacterPanelOpen = true;
        characterDetailsPanel.DisplayWithCharacter(BattleSystem.instance.GetActivePlayer());
    }

    public void CloseCharacterPanel()
    {
        CharacterPanelOpen = false;
        characterDetailsPanel.Hide();
    }

    private void UpdateSkillGraphic(PlayerCharacter obj)
    {
        if (BattleSystem.instance.CurrentPhase == BattlePhases.PlayerTurn)
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

        foreach (PlayerCharacter p in BattleSystem.instance.PlayerCharacters)
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
        BattleSystem.instance.GetActivePlayer().CancelSkill();
        ResumePlayerControl();
    }

    public void ExitSkillTargetMode()
    {
        SelectingAllyForSkill = false;

        skillBackButton.Hide();
        skillTargetMessage.Hide();

        foreach (PlayerCharacter p in BattleSystem.instance.PlayerCharacters)
        {
            p.HideSelectionPointer();
        }

        BattleSystem.instance.GetActiveEnemy().ShowSelectionPointer();
    }

    public void ShowSkillDetails(int index)
    {
        skillPanel.UpdateDetails(BattleSystem.instance.GetActivePlayer().GetSkill(index));
    }

    public void AttackPress()
    {
        onAttackCommit?.Invoke();
        BattleSystem.instance.ExecutePlayerAttack();
        offenseBar.InitializeBar(BattleSystem.instance.GetActivePlayer().GetAttackLeniency());
    }

    public void StartDefending()
    {
        defenseBar.InitializeBar(BattleSystem.instance.GetActivePlayer().GetDefenceLeniency());
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
        PlayButtonSound();
        RemovePlayerControl();
        optionsCanvas.Show();
        gameSpeedButton.Hide();
    }

    public void HideSettingsMenu()
    {
        PlayButtonSound();
        ResumePlayerControl();
        optionsCanvas.Hide();
        gameSpeedButton.Show();
    }

    private void UpdateWaveCounter()
    {
        waveCounter.text = (EnemyWaveManager.instance.CurrentWave + 1) + "/" + EnemyWaveManager.instance.TotalWaves;
    }

    public void UpdateGameSpeed()
    {
        gameSpeedText.text = BattleSystem.instance.CurrentGameSpeedTime + "x";
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