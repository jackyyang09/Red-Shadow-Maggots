using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickTimeDefense : QuickTimeBase
{
    [SerializeField] List<string> attackFunctionNames = new List<string>();

    [SerializeField] float barMinValue = 0.9f;
    [SerializeField] float barSuccessValue = 0.4f;
    [SerializeField] float barMissValue = 1;

    int attacks;
    List<AnimationEvent> animationEvents;
    List<BaseCharacter> targetPlayers;

    float startTime, attackLength;

    bool missed = true;

    public override void InitializeBar(BaseCharacter attacker, List<BaseCharacter> targets = null)
    {
        targetPlayers = targets;
        AnimationClip anim = attacker.Reference.attackAnimation;
        var events = anim.events;
        animationEvents = new List<AnimationEvent>();
        attacks = 0;
        missed = true;

        for (int i = 0; i < events.Length; i++)
        {
            if (attackFunctionNames.Contains(events[i].functionName))
            {
                animationEvents.Add(events[i]);
            }
        }

        attackLength = animationEvents[0].time;
        StartTicking();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Time.time - startTime) >= attackLength)
        {
            if (Input.GetMouseButtonDown(0))
            {
                missed = false;
            }

            CheckDefense();
        }
    }

    public void CheckDefense()
    {
        enabled = false;
        DamageStruct dmg = GetMultiplier();
        OnQuickTimeComplete?.Invoke(dmg);
        onExecuteQuickTime?.Invoke(dmg);
        for (int i = 0; i < targetPlayers.Count; i++)
        {
            ((PlayerCharacter)targetPlayers[i]).EndDefense();
        }
    }

    public override DamageStruct GetMultiplier()
    {
        DamageStruct damage = new DamageStruct();
        float timeElapsed = Time.time - startTime;
        float timeDiff = Mathf.Abs(timeElapsed - animationEvents[0].time);

        float leniency = targetPlayers[0].DefenseLeniency;
        for (int i = 1; i < targetPlayers.Count; i++) leniency += targetPlayers[i].DefenseLeniency;
        leniency /= targetPlayers.Count;

        float window = animationEvents[0].time * leniency;

        if (missed)
        {
            damage.damageNormalized = barMissValue;
            damage.qteResult = QTEResult.Late;
        }
        else if (timeDiff <= window)
        {
            damage.damageNormalized = barSuccessValue;
            damage.qteResult = QTEResult.Perfect;
            GlobalEvents.OnPlayerQuickTimeBlockSuccess?.Invoke();
        }
        else if (!missed)
        {
            damage.damageNormalized = Mathf.Lerp(barMinValue, barSuccessValue, Mathf.InverseLerp(0, window, timeDiff));
            damage.qteResult = QTEResult.Early;
        }
        
        return damage;
    }

    public override void StartTicking()
    {
        enabled = true;
        startTime = Time.time;
    }
}