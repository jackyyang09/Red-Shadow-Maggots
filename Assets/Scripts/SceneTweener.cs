using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SceneTweener : MonoBehaviour
{
    [SerializeField]
    Transform worldCenter;

    [SerializeField]
    Cinemachine.CinemachineVirtualCamera vCam;
    Cinemachine.CinemachineComposer composer;
    Cinemachine.CinemachineTrackedDolly dolly;

    [SerializeField]
    Transform path;

    [SerializeField]
    float tweenTime;

    [SerializeField]
    float camTweenTime;

    [SerializeField]
    float returnTweenDelay = 1;

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
        composer = vCam.GetCinemachineComponent<Cinemachine.CinemachineComposer>();
        dolly = vCam.GetCinemachineComponent<Cinemachine.CinemachineTrackedDolly>();
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //}

    // Update is called once per frame
    void Update()
    {
        dolly.m_PathPosition = lerpValue;
    }

    float lerpValue;

    Vector3 savedPosition;
    public void MeleeTweenTo(Transform attacker, Transform target)
    {
        vCam.m_LookAt = attacker;
        switch (BattleSystem.instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                attacker.transform.DOMove(target.position + new Vector3(2, 0, 0), tweenTime).SetEase(Ease.OutCubic);
                path.transform.DOMove(target.position + new Vector3(2, 0, 0), camTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late); ;
                break;
            case BattlePhases.EnemyTurn:
                attacker.transform.DOMove(target.position - new Vector3(2, 0, 0), tweenTime).SetEase(Ease.OutCubic);
                path.transform.DOMove(target.position - new Vector3(2, 0, 0), tweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late); ;
                break;
        }
        savedPosition = attacker.position;
        DOTween.To(() => lerpValue, x => lerpValue = x, 2, camTweenTime).SetEase(Ease.OutCubic);
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
        vCam.m_LookAt = BattleSystem.instance.GetActivePlayer().transform;

        yield return new WaitForSeconds(returnTweenDelay);

        switch (BattleSystem.instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                target.transform.DOMove(savedPosition, tweenTime).SetEase(Ease.OutCubic).onComplete += () => vCam.m_LookAt = worldCenter;
                DOTween.To(() => lerpValue, x => lerpValue = x, 0, camTweenTime).SetEase(Ease.OutCubic);
                path.transform.DOMove(Vector3.zero, camTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
                break;
            case BattlePhases.EnemyTurn:
                target.transform.DOMove(savedPosition, tweenTime).SetEase(Ease.OutCubic).onComplete += () => vCam.m_LookAt = worldCenter;
                DOTween.To(() => lerpValue, x => lerpValue = x, 0, camTweenTime).SetEase(Ease.OutCubic);
                path.transform.DOMove(Vector3.zero, camTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late); ;
                break;
        }
    }

    public void WaveClearSequence()
    {
        anim.enabled = true;
        ScreenEffects.instance.FadeToBlack(1.5f);
        anim.SetTrigger("OpenGate");
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
