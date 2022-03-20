using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OptimizedTransitionTween : OptimizedTransitionBase
{
    [System.Serializable]
    public class Tweeners
    {
        public RectTransform rect = null;
        public Vector2 outPosition;
        public Vector2 inPosition;
    }

    [SerializeField] float tweenInTime = 1;
    [SerializeField] float tweenOutTime = 1;

    [SerializeField] List<Tweeners> tweeners = new List<Tweeners>();

    [SerializeField] Ease easeType = Ease.Linear;

    Coroutine tweenRoutine;

    public override void EditorTransitionIn()
    {
        for (int i = 0; i < tweeners.Count; i++)
        {
            tweeners[i].rect.anchoredPosition = tweeners[i].inPosition;
        }
    }

    public override void EditorTransitionOut()
    {
        for (int i = 0; i < tweeners.Count; i++)
        {
            tweeners[i].rect.anchoredPosition = tweeners[i].outPosition;
        }
    }

    public override float GetTransitionInTime() => tweenInTime;

    public override float GetTransitionOutTime() => tweenOutTime;

    IEnumerator TweenIn()
    {
        for (int i = 0; i < tweeners.Count; i++)
        {
            tweeners[i].rect.DOAnchorPos(tweeners[i].inPosition, tweenInTime)
                .SetUpdate(ignoreTimescale)
                .SetEase(easeType);
        }
        if (ignoreTimescale) yield return new WaitForSecondsRealtime(tweenInTime);
        else yield return new WaitForSeconds(tweenInTime);
    }

    IEnumerator TweenOut()
    {
        for (int i = 0; i < tweeners.Count; i++)
        {
            tweeners[i].rect.DOAnchorPos(tweeners[i].outPosition, tweenInTime)
                .SetUpdate(ignoreTimescale)
                .SetEase(easeType);
        }
        if (ignoreTimescale) yield return new WaitForSecondsRealtime(tweenInTime);
        else yield return new WaitForSeconds(tweenInTime);
    }

    public override Coroutine TransitionIn()
    {
        if (tweenRoutine != null)
        {
            KillTween();
        }
        tweenRoutine = StartCoroutine(TweenIn());
        return tweenRoutine;
    }

    public override Coroutine TransitionOut()
    {
        if (tweenRoutine != null)
        {
            KillTween();
        }
        tweenRoutine = StartCoroutine(TweenOut());
        return tweenRoutine;
    }

    void KillTween()
    {
        StopCoroutine(tweenRoutine);
        for (int i = 0; i < tweeners.Count; i++)
        {
            tweeners[i].rect.DOKill(false);
        }
    }
}
