using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SceneTweener : MonoBehaviour
{
    [SerializeField]
    Transform worldCenter;

    [SerializeField]
    Cinemachine.CinemachineVirtualCamera playerCam;
    Cinemachine.CinemachineComposer composer;
    Cinemachine.CinemachineTrackedDolly playerDolly;

    [SerializeField]
    Cinemachine.CinemachineVirtualCamera enemyCam;
    Cinemachine.CinemachineTrackedDolly enemyDolly;

    [SerializeField]
    Cinemachine.CinemachineSmoothPath playerPath;

    [SerializeField]
    Cinemachine.CinemachineSmoothPath enemyPath;

    [SerializeField]
    float tweenTime;

    [SerializeField]
    float camTweenTime;

    [SerializeField]
    float returnTweenDelay = 1;

    [SerializeField] float waveTransitionDelay = 1;

    [SerializeField]
    float turnTransitionDelay = 3;
    public float TurnTransitionDelay
    {
        get
        {
            return turnTransitionDelay;
        }
    }

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
        ScreenEffects.instance.FadeFromBlack();

        GlobalEvents.onEnterWave?.Invoke();
        if (EnemyWaveManager.instance.IsLastWave) GlobalEvents.onEnterFinalWave?.Invoke();
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
                attacker.transform.DOMove(target.position + new Vector3(2, 0, 0), tweenTime).SetEase(Ease.OutCubic);
                playerPath.transform.DOMove(target.position + new Vector3(2, 0, 0), tweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.enabled = true;
                enemyCam.m_LookAt = attacker;
                attacker.transform.DOMove(target.position - new Vector3(2, 0, 0), tweenTime).SetEase(Ease.OutCubic);
                enemyPath.transform.DOMove(target.position - new Vector3(2, 0, 0), tweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
                break;
        }
        savedPosition = attacker.position;
        DOTween.To(() => lerpValue, x => lerpValue = x, 2, camTweenTime).SetEase(Ease.OutCubic);
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

    public void ReturnToPosition(Transform target)
    {
        StartCoroutine(ReturnToPositionDelayed(target));
    }

    IEnumerator ReturnToPositionDelayed(Transform target)
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
                target.transform.DOMove(savedPosition, tweenTime).SetEase(Ease.OutCubic).onComplete += () => playerCam.m_LookAt = worldCenter;
                DOTween.To(() => lerpValue, x => lerpValue = x, 0, camTweenTime).SetEase(Ease.OutCubic);
                playerPath.transform.DOMove(Vector3.zero, camTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
                break;
            case BattlePhases.EnemyTurn:
                target.transform.DOMove(savedPosition, tweenTime).SetEase(Ease.OutCubic).onComplete += () => { enemyCam.m_LookAt = worldCenter; enemyCam.enabled = false; };
                DOTween.To(() => lerpValue, x => lerpValue = x, 0, camTweenTime).SetEase(Ease.OutCubic);
                enemyPath.transform.DOMove(Vector3.zero, camTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late); ;
                break;
        }
    }

    public void WaveClearSequence()
    {
        anim.enabled = true;
        ScreenEffects.instance.FadeToBlack(1.5f);
        anim.SetTrigger("OpenGate");
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
