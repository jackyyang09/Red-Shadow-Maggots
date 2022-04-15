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

    int attacks;
    List<AnimationEvent> animationEvents;
    List<PlayerCharacter> targetPlayers;

    float startTime, attackLength;

    bool missed = true;

    public override void InitializeBar(BaseCharacter attacker, List<BaseCharacter> targets = null)
    {
    }

    public void InitializeDefenseBar()
    {
        var events = BaseCharacter.IncomingAttack.attackAnimation.events;
        animationEvents = new List<AnimationEvent>();
        attacks = 0;
        missed = true;

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
            targetPlayers = battleSystem.PlayerCharacters;
            for (int i = 0; i < targetPlayers.Count; i++)
            {
                targetPlayers[i].InitiateDefense();
            }
        }
        else
        {
            targetPlayers = new List<PlayerCharacter> { battleSystem.EnemyAttackTarget };
            battleSystem.EnemyAttackTarget.InitiateDefense();
        }

        canvas.Show();

        PrepareNextAttack();
    }

    public void PrepareNextAttack()
    {
        if (attacks == 0)
        {
            attackLength = animationEvents[attacks].time;
        }
        else
        {
            attackLength = animationEvents[attacks].time - animationEvents[attacks - 1].time;
        }
        StartTicking();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Time.time - startTime) >= attackLength)
        {
            if (Time.time - startTime < defenseBuffer) return;

            if (Input.GetMouseButtonDown(0))
            {
                missed = false;
            }

            CheckDefense();
        }
    }

    public void CheckDefense()
    {
        GetMultiplier();
        attacks++;
        onExecuteQuickTime?.Invoke();
        OnExecuteQuickTime?.Invoke();
        if (attacks == animationEvents.Count)
        {
            enabled = false;
            Invoke(nameof(Hide), hideDelay);
            OnQuickTimeComplete?.Invoke();
            for (int i = 0; i < targetPlayers.Count; i++)
            {
                ((PlayerCharacter)targetPlayers[i]).EndDefense();
            }
        }
        else PrepareNextAttack();
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
            BaseCharacter.IncomingDamage.damageNormalized = barMissValue;
            BaseCharacter.IncomingDamage.qteResult = QTEResult.Late;
        }
        else if (timeDiff <= window)
        {
            BaseCharacter.IncomingDamage.damageNormalized = barSuccessValue;
            BaseCharacter.IncomingDamage.qteResult = QTEResult.Perfect;
            GlobalEvents.OnPlayerQuickTimeBlockSuccess?.Invoke();
        }
        else if (!missed)
        {
            BaseCharacter.IncomingDamage.damageNormalized = Mathf.Lerp(barMinValue, barSuccessValue, Mathf.InverseLerp(0, window, timeDiff));
            BaseCharacter.IncomingDamage.qteResult = QTEResult.Early;
        }
    }

    public override void StartTicking()
    {
        enabled = true;
        startTime = Time.time;
    }
}