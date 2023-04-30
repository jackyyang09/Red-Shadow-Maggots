using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using static Facade;

public class SceneTweener : BasicSingleton<SceneTweener>
{
    [SerializeField] Transform worldCenter;

    [SerializeField] CinemachineVirtualCamera playerCam;

    [SerializeField] Camera sceneCamera;
    public Camera SceneCamera { get { return sceneCamera; } }

    /// <summary>
    /// The player-centric camera
    /// </summary>
    public CinemachineVirtualCamera PlayerCamera { get { return playerCam; } }

    [SerializeField] CinemachineVirtualCamera lerpCam;
    public CinemachineVirtualCamera LerpCamera { get { return lerpCam; } }

    CinemachineComposer composer;
    CinemachineTrackedDolly playerDolly;

    [SerializeField] CinemachineVirtualCamera enemyCam;
    CinemachineTrackedDolly enemyDolly;

    [SerializeField] CinemachineVirtualCamera specialCam;

    [SerializeField] CinemachineSmoothPath playerPath;

    [SerializeField] CinemachineSmoothPath enemyPath;

    [SerializeField] Vector3 characterDestinationOffset = new Vector3();

    [SerializeField] float characterTweenDelay = 0.15f;
    [SerializeField] float characterTweenTime = 0.5f;

    [SerializeField] float tweenTime = 0;

    [SerializeField] float camTweenTime = 0;

    [SerializeField] float returnTweenDelay = 1;

    [SerializeField] float waveTransitionDelay = 1;

    [SerializeField] float enemyTurnTransitionDelay = 2;
    public float EnemyTurnTransitionDelay { get { return enemyTurnTransitionDelay; } }

    [SerializeField] float playerTurnTransitionDelay = 2;
    public float PlayerTurnTransitionDelay { get { return playerTurnTransitionDelay; } }

    Animator anim;

    public static Action OnBattleTransition;
    public static Action OnSkillUntween;
    public static Action OnBattleEntered;

    private void OnEnable()
    {
        GlobalEvents.OnCharacterUseSuperCritical += OnCharacterUseSuperCritical;
        //GlobalEvents.OnCharacterFinishSuperCritical += OnCharacterFinishSuperCritical;
        ui.OffenseBar.OnExecuteQuickTime += OnExecuteQuickTime;
    }

    private void OnDisable()
    {
        GlobalEvents.OnCharacterUseSuperCritical -= OnCharacterUseSuperCritical;
        //GlobalEvents.OnCharacterFinishSuperCritical -= OnCharacterFinishSuperCritical;

        // References may be lost on Application Quit
        if (ui)
        {
            if (ui.OffenseBar)
            {
                ui.OffenseBar.OnExecuteQuickTime -= OnExecuteQuickTime;
            }
        }
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        composer = playerCam.GetCinemachineComponent<CinemachineComposer>();
        playerDolly = playerCam.GetCinemachineComponent<CinemachineTrackedDolly>();

        enemyDolly = enemyCam.GetCinemachineComponent<CinemachineTrackedDolly>();
        //EnterBattle();
    }

    // Update is called once per frame
    void Update()
    {
        playerDolly.m_PathPosition = lerpValue;
        enemyDolly.m_PathPosition = lerpValue;
    }

    public void EnterBattleAfterDelay() => Invoke(nameof(EnterBattle), waveTransitionDelay);

    public void EnterBattle()
    {
        screenEffects.FadeFromBlack(ScreenEffects.EffectType.Fullscreen);

        bool useSpecialCam = playerDataManager.LoadedData.InBattle ?
            battleStateManager.LoadedData.UseSpecialCam[battleStateManager.LoadedData.WaveCount] :
            waveManager.CurrentWave.UseSpecialCam;

        playerCam.enabled = !useSpecialCam;
        specialCam.enabled = useSpecialCam;

        OnBattleTransition?.Invoke();
        OnBattleEntered?.Invoke();
    }

    float lerpValue;

    Vector3 savedPosition;
    public void MeleeTweenTo(Transform attacker, Transform target)
    {
        switch (BattleSystem.Instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                enemyCam.enabled = false;
                playerCam.m_LookAt = attacker;
                attacker.transform.DOMove(target.position + characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic);
                //playerPath.transform.DOKill();
                //playerPath.transform.DOMove(target.position + characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.enabled = true;
                enemyCam.m_LookAt = attacker;
                attacker.transform.DOMove(target.position - characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic);
                enemyPath.transform.DOKill();
                enemyPath.transform.DOMove(target.position - characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
                break;
        }
        savedPosition = attacker.position;
        DOTween.To(() => lerpValue, x => lerpValue = x, 2, camTweenTime).SetEase(Ease.OutCubic);
    }

    public void RangedTweenTo(Transform attacker, Transform target)
    {
        switch (BattleSystem.Instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                playerCam.m_LookAt = battleSystem.ActivePlayer.transform;
                attacker.LookAt(target.position);
                break;
            case BattlePhases.EnemyTurn:
                if (!BaseCharacter.IncomingAttack.isAOE)
                    attacker.LookAt(target.position);
                break;
        }
    }

    public void SkillTween(Transform user, float skillUseTime)
    {
        //playerCam.m_LookAt = user;
        //playerPath.transform.position = user.position;
        //DOTween.To(() => lerpValue, x => lerpValue = x, 1.25f, skillUseTime).SetEase(Ease.OutCubic);
    }

    public void SkillUntween()
    {
        lerpCam.enabled = false;
        OnSkillUntween?.Invoke();
        //playerCam.m_LookAt = worldCenter;
        //playerPath.transform.position = worldCenter.position;
        //DOTween.To(() => lerpValue, x => lerpValue = x, 0, 1.5f);
    }

    private void OnCharacterUseSuperCritical(BaseCharacter obj)
    {
        Transform target = null;
        Vector3 offset = Vector3.zero;
        CinemachineVirtualCamera cam;
        CinemachineSmoothPath path;
        switch (battleSystem.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                enemyCam.enabled = false;
                path = playerPath;
                cam = playerCam;
                target = battleSystem.ActiveEnemy.transform;
                offset = characterDestinationOffset;
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.enabled = false;
                path = enemyPath;
                cam = enemyCam;
                target = battleSystem.ActivePlayer.transform;
                offset = -characterDestinationOffset;
                break;
            default:
                return;
        }

        switch (obj.Reference.attackAnimations[0].attackRange)
        {
            case AttackRange.CloseRange:
                cam.m_LookAt = obj.transform;
                savedPosition = obj.transform.position;
                Vector3 position = target.position + offset;
                //obj.transform.position = position;
                path.transform.position = position;
                lerpValue = 2;
                break;
            case AttackRange.LongRange:
                obj.CharacterMesh.transform.LookAt(target.position);
                break;
        }
    }

    public void DisableAnim()
    {
        anim.enabled = false;
    }

    public void ReturnToPosition()
    {
        StartCoroutine(ReturnToPositionDelayed());
    }

    private void OnExecuteQuickTime()
    {
        // Look at enemy whose getting hit by the attack
        if (BaseCharacter.IncomingAttack.attackRange == AttackRange.LongRange)
        {
            playerCam.m_LookAt = battleSystem.ActiveEnemy.transform;
        }
    }

    public void RotateBackInstantly()
    {
        battleSystem.ActiveEnemy.CharacterMesh.transform.eulerAngles = new Vector3(0, 90, 0);
        battleSystem.ActivePlayer.CharacterMesh.transform.eulerAngles = new Vector3(0, -90, 0);
    }

    public void RotateBack()
    {
        StartCoroutine(RotateBackDelayed());
    }

    IEnumerator RotateBackDelayed()
    {
        switch (battleSystem.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                playerCam.m_LookAt = battleSystem.ActivePlayer.transform;
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.m_LookAt = battleSystem.ActivePlayer.transform;
                break;
        }

        yield return new WaitForSeconds(returnTweenDelay);

        switch (battleSystem.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                var activePlayer = battleSystem.ActivePlayer;

                battleSystem.ActiveEnemy.CharacterMesh.transform.DORotate(new Vector3(0, 90, 0), 0.15f);
                activePlayer.CharacterMesh.transform.DORotate(new Vector3(0, -90, 0), 0.1f);
                break;
            case BattlePhases.EnemyTurn:
                var activeEnemy = battleSystem.ActiveEnemy;

                battleSystem.ActivePlayer.CharacterMesh.transform.DORotate(new Vector3(0, -90, 0), 0.15f);
                activeEnemy.CharacterMesh.transform.DORotate(new Vector3(0, 90, 0), 0.1f);
                break;
        }

        playerCam.m_LookAt = worldCenter;

        battleSystem.EndTurn();
    }

    IEnumerator ReturnToPositionDelayed()
    {
        switch (BattleSystem.Instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                playerCam.m_LookAt = battleSystem.ActivePlayer.transform;
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.m_LookAt = battleSystem.ActiveEnemy.transform;
                break;
        }

        yield return new WaitForSeconds(returnTweenDelay);

        bool maintainGaze = true;
        Vector3 ogPos = Vector3.zero;
        Vector3 ogRot = Vector3.zero;
        switch (battleSystem.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                var activePlayer = battleSystem.ActivePlayer;

                ogPos = activePlayer.CharacterMesh.transform.position;
                ogRot = activePlayer.CharacterMesh.transform.eulerAngles;
                activePlayer.PlayReturnAnimation();

                DOTween.To(() => lerpValue, x => lerpValue = x, 0, camTweenTime).SetEase(Ease.OutCubic);
                //playerPath.transform.DOKill();
                //playerPath.transform.DOMove(Vector3.zero, camTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);

                activePlayer.transform.DOMove(savedPosition, characterTweenTime).SetDelay(characterTweenDelay).SetEase(Ease.Linear).onComplete += () =>
                {
                    activePlayer.CharacterMesh.transform.DORotate(ogRot, 0.15f, RotateMode.Fast);
                    playerCam.m_LookAt = worldCenter;
                    maintainGaze = false;
                };

                while (maintainGaze)
                {
                    Vector3 targetDirection = ogPos - activePlayer.CharacterMesh.transform.position;
                    targetDirection.Normalize();

                    activePlayer.CharacterMesh.transform.forward = 
                        Vector3.RotateTowards(activePlayer.CharacterMesh.transform.forward, targetDirection, 2 * Time.deltaTime, 0);

                    yield return null;
                }
                break;
            case BattlePhases.EnemyTurn:
                var activeEnemy = battleSystem.ActiveEnemy;

                ogPos = activeEnemy.CharacterMesh.transform.position;
                ogRot = activeEnemy.CharacterMesh.transform.eulerAngles;
                activeEnemy.PlayReturnAnimation();

                DOTween.To(() => lerpValue, x => lerpValue = x, 0, camTweenTime).SetEase(Ease.OutCubic);
                enemyPath.transform.DOKill();
                enemyPath.transform.DOMove(Vector3.zero, camTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);

                activeEnemy.transform.DOMove(savedPosition, characterTweenTime).SetDelay(characterTweenDelay).SetEase(Ease.OutCubic).onComplete += () => {
                    activeEnemy.CharacterMesh.transform.DORotate(ogRot, 0.15f, RotateMode.Fast);
                    enemyCam.m_LookAt = worldCenter;
                    enemyCam.enabled = false;
                    maintainGaze = false;
                };

                while (maintainGaze)
                {
                    Vector3 targetDirection = ogPos - activeEnemy.CharacterMesh.transform.position;
                    targetDirection.Normalize();

                    activeEnemy.CharacterMesh.transform.forward =
                        Vector3.RotateTowards(activeEnemy.CharacterMesh.transform.forward, targetDirection, 2 * Time.deltaTime, 0);

                    yield return null;
                }
                break;
        }

        battleSystem.EndTurn();
    }

    public void WaveClearSequence()
    {
        OnBattleTransition?.Invoke();
        anim.enabled = true;
        screenEffects.FadeToBlack(ScreenEffects.EffectType.Fullscreen, 1.5f);
        anim.SetTrigger("OpenGate");
    }

    public void GateEntered()
    {
        BattleSystem.Instance.InitiateNextBattle();
    }
}
