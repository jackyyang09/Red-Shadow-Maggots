using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    OptimizedButton attackButton;

    [SerializeField]
    QuickTimeBar offenseBar;

    [SerializeField]
    QuickTimeBar defenseBar;

    [SerializeField]
    OptimizedCanvas winCanvas;

    [SerializeField]
    OptimizedCanvas loseCanvas;

    public static bool CanSelect = true;

    public static System.Action onAttackCommit;

    public static UIManager instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        BattleSystem.onStartPlayerTurn += ResumePlayerControl;
        GlobalEvents.onPlayerDefeat += ShowLoseCanvas;
        GlobalEvents.onFinalWaveClear += ShowWinCanvas;

        onAttackCommit += RemovePlayerControl;
    }

    private void OnDisable()
    {
        BattleSystem.onStartPlayerTurn -= ResumePlayerControl;
        GlobalEvents.onPlayerDefeat -= ShowLoseCanvas;
        GlobalEvents.onFinalWaveClear -= ShowWinCanvas;

        onAttackCommit -= RemovePlayerControl;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public void ResumePlayerControl()
    {
        attackButton.Show();
        CanSelect = true;
    }

    public void RemovePlayerControl()
    {
        attackButton.Hide();
        CanSelect = false;
    }

    public void AttackPress()
    {
        onAttackCommit?.Invoke();
        BattleSystem.instance.ExecutePlayerAttack();
        offenseBar.InitializeBar(BattleSystem.instance.GetActivePlayer().GetAttackLeniency());
    }

    public void StartDefending()
    {
        defenseBar.InitializeBar(BattleSystem.instance.GetActivePlayer().GetDefenseLeniency());
    }

    public void ShowWinCanvas()
    {
        winCanvas.SetActive(true);
    }

    public void ShowLoseCanvas()
    {
        loseCanvas.SetActive(true);
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