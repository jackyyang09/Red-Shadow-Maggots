using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OptimizedTransitionSlide : OptimizedTransitionBase
{
    [System.Serializable]
    public struct SlideRect
    {
        public RectTransform rect;
        public Vector2 origin;
    }

    [System.Serializable]
    public struct SlideStruct
    {
        public SlideRect[] rects;
        public float slideAngle;
        public float distance;
    }

    [Header("Transition Properties")]
    [SerializeField] float transitionTime = 0.5f;
    [SerializeField] SlideStruct[] sliders = null;
    [SerializeField] Ease easeType = Ease.Linear;

    public override Coroutine TransitionIn()
    {
        return StartCoroutine(SlideTransitionIn());
    }

    public override Coroutine TransitionOut()
    {
        return StartCoroutine(SlideTransitionOut());
    }

    IEnumerator SlideTransitionIn()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            for (int j = 0; j < sliders[i].rects.Length; j++)
            {
                sliders[i].rects[j].rect.DOAnchorPos(sliders[i].rects[j].origin, transitionTime).SetEase(easeType);
            }
        }
        if (ignoreTimescale) yield return new WaitForSecondsRealtime(transitionTime);
        else yield return new WaitForSeconds(transitionTime);
    }

    IEnumerator SlideTransitionOut()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            for (int j = 0; j < sliders[i].rects.Length; j++)
            {
                // SOH CAH TOA review!
                Vector2 destination = sliders[i].rects[j].origin + new Vector2(
                    sliders[i].distance * Mathf.Cos(sliders[i].slideAngle * Mathf.Deg2Rad),
                    sliders[i].distance * Mathf.Sin(sliders[i].slideAngle * Mathf.Deg2Rad)
                    );
                sliders[i].rects[j].rect.DOAnchorPos(destination, transitionTime).SetEase(easeType);
            }
        }
        if (ignoreTimescale) yield return new WaitForSecondsRealtime(transitionTime);
        else yield return new WaitForSeconds(transitionTime);
    }

    public override void EditorTransitionIn()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            for (int j = 0; j < sliders[i].rects.Length; j++)
            {
                sliders[i].rects[j].rect.anchoredPosition = sliders[i].rects[j].origin;
            }
        }
    }

    public override void EditorTransitionOut()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            for (int j = 0; j < sliders[i].rects.Length; j++)
            {
                // SOH CAH TOA review!
                Vector2 destination = sliders[i].rects[j].origin + new Vector2(
                    sliders[i].distance * Mathf.Cos(sliders[i].slideAngle * Mathf.Deg2Rad),
                    sliders[i].distance * Mathf.Sin(sliders[i].slideAngle * Mathf.Deg2Rad)
                    );
                sliders[i].rects[j].rect.anchoredPosition = destination;
            }
        }
    }
}