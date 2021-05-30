using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SceneTweener : MonoBehaviour
{
    [SerializeField] Transform worldCenter = null;

    [SerializeField] Cinemachine.CinemachineVirtualCamera playerCam = null;
    /// <summary>
    /// The player-centric camera
    /// </summary>
    public Cinemachine.CinemachineVirtualCamera PlayerCamera { get { return playerCam; } }

    Cinemachine.CinemachineComposer composer;
    Cinemachine.CinemachineTrackedDolly playerDolly;

    [SerializeField] Cinemachine.CinemachineVirtualCamera enemyCam = null;
    Cinemachine.CinemachineTrackedDolly enemyDolly;

    [SerializeField] Cinemachine.CinemachineSmoothPath playerPath = null;

    [SerializeField] Cinemachine.CinemachineSmoothPath enemyPath = null;

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

    public static SceneTweener instance;

    private void Awake()
    {
        EstablishSingletonDominance();

        anim = GetComponent<Animator>();
        composer = playerCam.GetCinemachineComponent<Cinemachine.CinemachineComposer>();
        playerDolly = playerCam.GetCinemachineComponent<Cinemachine.CinemachineTrackedDolly>();

        enemyDolly = enemyCam.GetCinemachineComponent<Cinemachine.CinemachineTrackedDolly>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //EnterBattle();
    }

    // Update is called once per frame
    void Update()
    {
        playerDolly.m_PathPosition = lerpValue;
        enemyDolly.m_PathPosition = lerpValue;
    }

    public void EnterBattle() => Invoke("EnterBattleAfterDelay", waveTransitionDelay);

    public void EnterBattleAfterDelay()
    {
        anim.SetTrigger("EnterBattle");

        MakePlayersWalk(1);

        ScreenEffects.instance.FadeFromBlack();

        GlobalEvents.OnEnterWave?.Invoke();
        if (EnemyWaveManager.instance.IsLastWave) GlobalEvents.OnEnterFinalWave?.Invoke();
    }

    float lerpValue;

    Vector3 savedPosition;
    public void MeleeTweenTo(Transform attacker, Transform target)
    {
        switch (BattleSystem.instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                enemyCam.enabled = false;
                playerCam.m_LookAt = attacker;
                attacker.transform.DOMove(target.position + characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic);
                playerPath.transform.DOMove(target.position + characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.enabled = true;
                enemyCam.m_LookAt = attacker;
                attacker.transform.DOMove(target.position - characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic);
                enemyPath.transform.DOMove(target.position - characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
                break;
        }
        savedPosition = attacker.position;
        DOTween.To(() => lerpValue, x => lerpValue = x, 2, camTweenTime).SetEase(Ease.OutCubic);
    }

    public void RangedTweenTo(Transform attacker, Transform target)
    {
        switch (BattleSystem.instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                attacker.LookAt(target.position);
                break;
            case BattlePhases.EnemyTurn:
                break;
        }
    }

    public void SkillTween(Transform user, float skillUseTime)
    {
        playerCam.m_LookAt = user;
        playerPath.transform.position = user.position;
        DOTween.To(() => lerpValue, x => lerpValue = x, 1.25f, skillUseTime).SetEase(Ease.OutCubic);
    }

    public void SkillUntween()
    {
        playerCam.m_LookAt = worldCenter;
        playerPath.transform.position = worldCenter.position;
        DOTween.To(() => lerpValue, x => lerpValue = x, 0, 1.5f);
    }

    public void DisableAnim()
    {
        anim.enabled = false;
        BattleSystem.instance.SetPhase(BattlePhases.PlayerTurn);
    }

    public void ReturnToPosition()
    {
        StartCoroutine(ReturnToPositionDelayed());
    }

    public void RotateBack()
    {
        StartCoroutine(RotateBackDelayed());
    }

    IEnumerator RotateBackDelayed()
    {
        switch (BattleSystem.instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                playerCam.m_LookAt = BattleSystem.instance.GetActivePlayer().transform;
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.m_LookAt = BattleSystem.instance.GetActivePlayer().transform;
                break;
        }

        yield return new WaitForSeconds(returnTweenDelay);

        switch (BattleSystem.instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                var activePlayer = BattleSystem.instance.GetActivePlayer();

                BattleSystem.instance.GetActiveEnemy().CharacterMesh.transform.DORotate(new Vector3(0, 90, 0), 0.15f);
                activePlayer.CharacterMesh.transform.DORotate(new Vector3(0, -90, 0), 0.1f);
                break;
            case BattlePhases.EnemyTurn:
                break;
        }

        playerCam.m_LookAt = worldCenter;

        BattleSystem.instance.EndTurn();
    }

    IEnumerator ReturnToPositionDelayed()
    {
        switch (BattleSystem.instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                playerCam.m_LookAt = BattleSystem.instance.GetActivePlayer().transform;
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.m_LookAt = BattleSystem.instance.GetActiveEnemy().transform;
                break;
        }

        yield return new WaitForSeconds(returnTweenDelay);

        bool maintainGaze = true;
        Vector3 ogPos = Vector3.zero;
        Vector3 ogRot = Vector3.zero;
        switch (BattleSystem.instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                var activePlayer = BattleSystem.instance.GetActivePlayer();

                ogPos = activePlayer.CharacterMesh.transform.position;
                ogRot = activePlayer.CharacterMesh.transform.eulerAngles;
                activePlayer.PlayReturnAnimation();

                DOTween.To(() => lerpValue, x => lerpValue = x, 0, camTweenTime).SetEase(Ease.OutCubic);
                playerPath.transform.DOMove(Vector3.zero, camTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);

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
                var activeEnemy = BattleSystem.instance.GetActiveEnemy();

                ogPos = activeEnemy.CharacterMesh.transform.position;
                ogRot = activeEnemy.CharacterMesh.transform.eulerAngles;
                activeEnemy.PlayReturnAnimation();

                DOTween.To(() => lerpValue, x => lerpValue = x, 0, camTweenTime).SetEase(Ease.OutCubic);
                enemyPath.transform.DOMove(Vector3.zero, camTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);

                activeEnemy.transform.DOMove(savedPosition, characterTweenTime + characterTweenDelay).SetDelay(characterTweenDelay).SetEase(Ease.OutCubic).onComplete += () => {
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

        BattleSystem.instance.EndTurn();
    }

    public void WaveClearSequence()
    {
        anim.enabled = true;
        ScreenEffects.instance.FadeToBlack(1.5f);
        anim.SetTrigger("OpenGate");
    }

    public void MakePlayersWalk(float walkTime)
    {
        for (int i = 0; i < BattleSystem.instance.PlayerCharacters.Count; i++)
        {
            var player = BattleSystem.instance.PlayerCharacters[i];
            player.AnimHelper.WalkForward(1);
        }
    }

    public void GateEntered()
    {
        BattleSystem.instance.InitiateNextBattle();
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
