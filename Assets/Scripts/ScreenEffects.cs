using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenEffects : MonoBehaviour
{
    [SerializeField]
    Image blackOut;

    public static ScreenEffects instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    //// Start is called before the first frame update
    //void Start()
    //{
    //    
    //}
    //
    //// Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public void FadeFromBlack()
    {
        blackOut.enabled = true;
        blackOut.color = Color.black;
        blackOut.DOFade(0, 1.5f).onComplete += () => blackOut.enabled = false;
    }

    public void FadeToBlack(float delay = 0)
    {
        blackOut.enabled = true;
        blackOut.color = Color.clear;
        blackOut.DOFade(1, 1.5f).SetDelay(delay).onComplete += () => blackOut.enabled = true;
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
