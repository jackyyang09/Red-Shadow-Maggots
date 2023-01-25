using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class OptimizedTransitionFade : OptimizedTransitionBase
{
    [SerializeField] float fadeInTime = 1;
    [SerializeField] float fadeOutTime = 1;
    [SerializeField] Ease easeType = Ease.Linear;

    [SerializeField] CanvasGroup canvasGroup = null;

    public override float GetTransitionInTime() => fadeInTime;
    public override float GetTransitionOutTime() => fadeOutTime;

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
        OnTransitionInStartUnityEvent?.Invoke();
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
        InvokeOnTransitionIn();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    IEnumerator FadeTransitionOut()
    {
        OnTransitionOutStartUnityEvent?.Invoke();
        if (ignoreTimescale)
        {
            canvasGroup.DOFade(0, fadeOutTime).SetEase(easeType).timeScale = 1;
            yield return new WaitForSecondsRealtime(fadeOutTime);
        }
        else
        {
            canvasGroup.DOFade(0, fadeOutTime).SetEase(easeType);
            yield return new WaitForSeconds(fadeOutTime);
        }
        InvokeOnTransitionOut();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
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
