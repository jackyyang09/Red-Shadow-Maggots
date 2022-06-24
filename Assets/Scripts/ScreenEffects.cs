using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenEffects : BasicSingleton<ScreenEffects>
{
    [SerializeField] GraphicRaycaster raycaster;
    [SerializeField] Image fadeImage = null;

    public void BlackOut()
    {
        fadeImage.color = Color.black;
        fadeImage.enabled = true;
        raycaster.enabled = true;
        raycaster.enabled = true;
    }

    public void FadeFromBlack(float fadeTime = 1.5f, System.Action onComplete = null)
    {
        StartCoroutine(FadeFromBlackRoutine(fadeTime, onComplete));
    }

    public IEnumerator FadeFromBlackRoutine(float fadeTime = 1.5f, System.Action onComplete = null)
    {
        fadeImage.color = Color.black;
        yield return FadeOutRoutine(fadeTime, onComplete);
    }

    public void FadeToBlack(float fadeTime = 1.5f, System.Action onComplete = null)
    {
        StartCoroutine(FadeToBlackRoutine(fadeTime, onComplete));
    }

    public IEnumerator FadeToBlackRoutine(float fadeTime = 1.5f, System.Action onComplete = null)
    {
        fadeImage.color = Color.black;
        yield return FadeInRoutine(fadeTime, onComplete);
    }

    public void FadeFromWhite(float fadeTime = 1.5f, System.Action onComplete = null)
    {
        StartCoroutine(FadeFromWhiteRoutine(fadeTime, onComplete));
    }

    public IEnumerator FadeFromWhiteRoutine(float fadeTime = 1.5f, System.Action onComplete = null)
    {
        fadeImage.color = Color.white;
        yield return FadeOutRoutine(fadeTime, onComplete);
    }

    public void FadeToWhite(float fadeTime = 1.5f, System.Action onComplete = null)
    {
        StartCoroutine(FadeToWhiteRoutine(fadeTime, onComplete));
    }

    public IEnumerator FadeToWhiteRoutine(float fadeTime = 1.5f, System.Action onComplete = null)
    {
        fadeImage.color = Color.white;
        yield return FadeInRoutine(fadeTime, onComplete);
    }

    IEnumerator FadeInRoutine(float fadeTime = 1.5f, System.Action onComplete = null)
    {
        fadeImage.enabled = true;
        raycaster.enabled = true;
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);
        fadeImage.DOFade(1, fadeTime);

        yield return new WaitForSeconds(fadeTime);
    }

    IEnumerator FadeOutRoutine(float fadeTime = 1.5f, System.Action onComplete = null)
    {
        fadeImage.enabled = true;
        raycaster.enabled = true;
        fadeImage.DOFade(0, fadeTime);

        yield return new WaitForSeconds(fadeTime);

        fadeImage.enabled = false;
        raycaster.enabled = false;
    }
}