using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SceneTweener : MonoBehaviour
{
    [SerializeField]
    Transform enemyPositions;

    [SerializeField]
    Cinemachine.CinemachineVirtualCamera vCam;

    [SerializeField]
    Transform path;

    [SerializeField]
    float tweenTime;

    [SerializeField]
    float camTweenTime;

    Animator anim;

    public static SceneTweener instance;

    private void Awake()
    {
        EstablishSingletonDominance();

        anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("CinemachinePath", lerpValue);
    }

    float lerpValue;

    public void MeleeTweenTo(Transform attacker, Transform target)
    {
        attacker.transform.DOMove(target.position + new Vector3(2, 0, 0), tweenTime).SetEase(Ease.OutCubic);
        DOTween.To(() => lerpValue, x => lerpValue = x, 1, camTweenTime).SetEase(Ease.OutCubic);
        path.transform.DOMove(target.position + new Vector3(2, 0, 0), camTweenTime).SetEase(Ease.OutCubic);
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
