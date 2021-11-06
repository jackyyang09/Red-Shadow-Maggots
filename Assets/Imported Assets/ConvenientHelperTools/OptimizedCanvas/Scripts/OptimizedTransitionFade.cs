using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OptimizedTransitionFade : OptimizedTransitionBase
{
    [SerializeField] float fadeInTime = 0;
    [SerializeField] float fadeOutTime = 0;
    [SerializeField] Ease easeType = Ease.Linear;

    [SerializeField] CanvasGroup canvasGroup = null;

    public override Coroutine TransitionIn()
    {
        return StartCoroutine(FadeTransitionIn());
    }

    public override Coroutine TransitionOut()
    {
        return StartCoroutine(FadeTransitionOut());
    }

    IEnumerator FadeTransitionIn()
    {
        if (ignoreTimescale)
        {
            canvasGroup.DOFade(1, fadeInTime).SetEase(easeType).timeScale = 1;
            yield return new WaitForSecondsRealtime(fadeInTime);
        }
        else
        {
            canvasGroup.DOFade(1, fadeInTime).SetEase(easeType);
            yield return new WaitForSeconds(fadeInTime);
        }
    }

    IEnumerator FadeTransitionOut()
    {
        if (ignoreTimescale)
        {
            canvasGroup.DOFade(0, fadeOutTime).SetEase(easeType).timeScale = 1;
            yield return new WaitForSecondsRealtime(fadeInTime);
        }
        else
        {
            canvasGroup.DOFade(0, fadeOutTime).SetEase(easeType);
            yield return new WaitForSeconds(fadeInTime);
        }
    }

    public override void EditorTransitionIn()
    {
        canvasGroup.alpha = 1;
    }

    public override void EditorTransitionOut()
    {
        canvasGroup.alpha = 0;
    }
}
