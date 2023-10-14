using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class QuickTimeDefense : QuickTimeBase
{
    [SerializeField] List<string> attackFunctionNames = new List<string>();

    [SerializeField] float barMinValue = 0.9f;
    [SerializeField] float barSuccessValue = 0.4f;
    [SerializeField] float barMissValue = 1;

    [SerializeField] float defenseBuffer = 0.1f;

    [SerializeField] OptimizedCanvas helpCanvas;

    int attacks;
    List<AnimationEvent> animationEvents;
    List<PlayerCharacter> targetPlayers;

    float startTime, attackLength;

    bool missed = true;

    public override void InitializeBar(BaseCharacter attacker, List<BaseCharacter> targets = null)
    {
    }

    protected override void Start()
    {
        base.Start();
        BattleSystem.OnEndPhase[BattlePhases.EnemyTurn.ToInt()] += Hide;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        BattleSystem.OnEndPhase[BattlePhases.EnemyTurn.ToInt()] -= Hide;
    }

    public void InitializeDefenseBar()
    {
        var events = BaseCharacter.IncomingAttack.attackAnimation.events;
        animationEvents = new List<AnimationEvent>();
        attacks = 0;

        bool isAOE = false;
        for (int i = 0; i < events.Length; i++)
        {
            if (attackFunctionNames.Contains(events[i].functionName))
            {
                if (events[i].functionName.Contains("AOE")) isAOE = true;
                animationEvents.Add(events[i]);
            }
        }

        if (isAOE)
        {
            targetPlayers = battleSystem.PlayerCharacters.Where(t => t != null).ToList();
            for (int i = 0; i < targetPlayers.Count; i++)
            {
                if (targetPlayers[i]) targetPlayers[i].InitiateDefense();
            }
        }
        else
        {
            targetPlayers = new List<PlayerCharacter> { battleSystem.EnemyAttackTarget };
            battleSystem.EnemyAttackTarget.InitiateDefense();
        }

        canvas.Show();

        Enable();
        PrepareNextAttack();

        helpCanvas.Show();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - startTime < defenseBuffer) return;

            missed = false;

            CheckDefense();
        }

        if ((Time.time - startTime) >= attackLength)
        {
            if (missed) CheckDefense();
            attacks++;
            PrepareNextAttack();
        }
    }

    public void PrepareNextAttack()
    {
        missed = true;
        if (attacks == 0)
        {
            attackLength = animationEvents[attacks].time;
        }
        else if (attacks < animationEvents.Count)
        {
            attackLength = animationEvents[attacks].time - animationEvents[attacks - 1].time;
        }
        else
        {
            enabled = false;
            return;
        }
        StartTicking();
    }

    public void CheckDefense()
    {
        GetMultiplier();
        OnExecuteAnyQuickTime?.Invoke();
        OnExecuteQuickTime?.Invoke();
        if (attacks == animationEvents.Count - 1)
        {
            enabled = false;
            Invoke(nameof(Hide), hideDelay);
            OnQuickTimeComplete?.Invoke();
            for (int i = 0; i < targetPlayers.Count; i++)
            {
                ((PlayerCharacter)targetPlayers[i]).EndDefense();
            }
        }
        helpCanvas.Hide();
    }

    public override void GetMultiplier()
    {
        float timeElapsed = Time.time - startTime;
        float timeDiff = Mathf.Abs(timeElapsed - attackLength);

        float leniency = targetPlayers[0].DefenseLeniency;
        for (int i = 1; i < targetPlayers.Count; i++) leniency += targetPlayers[i].DefenseLeniency;
        leniency /= targetPlayers.Count;

        float window = leniency;

        if (missed)
        {
            BaseCharacter.IncomingDamage.DamageNormalized = barMissValue;
            BaseCharacter.IncomingDamage.QTEResult = QTEResult.Late;
        }
        else if (timeDiff <= window)
        {
            BaseCharacter.IncomingDamage.DamageNormalized = barSuccessValue;
            BaseCharacter.IncomingDamage.QTEResult = QTEResult.Perfect;
            GlobalEvents.OnPlayerQuickTimeBlockSuccess?.Invoke();
        }
        else if (!missed)
        {
            BaseCharacter.IncomingDamage.DamageNormalized = Mathf.Lerp(barMinValue, barSuccessValue, Mathf.InverseLerp(0, window, timeDiff));
            BaseCharacter.IncomingDamage.QTEResult = QTEResult.Early;
        }
    }

    public override void StartTicking()
    {
        startTime = Time.time;
    }
}