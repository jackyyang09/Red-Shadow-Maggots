using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenEffects : BasicSingleton<ScreenEffects>
{
    public enum EffectType
    { 
        Fullscreen,
        Partial
    }

    [System.Serializable]
    public class EffectGroup
    {
        public CanvasGroup CanvasGroup;
        public Image Image;
        public GraphicRaycaster Raycaster;
    }

    [Header("Fullscreen Effects")]
    [SerializeField] EffectGroup fullScreenGroup;

    [Header("Partial Effects")]
    [SerializeField] EffectGroup partialGroup;

    [SerializeField] float rotateSpeed;
    [SerializeField] Image loadingIcon;

    Dictionary<EffectType, EffectGroup> effectDictionary = new Dictionary<EffectType, EffectGroup>();

    private void Start()
    {
        effectDictionary.Add(EffectType.Fullscreen, fullScreenGroup);
        effectDictionary.Add(EffectType.Partial, partialGroup);
    }

    public void BlackOut(EffectType type)
    {
        effectDictionary[type].Image.color = Color.black;
        effectDictionary[type].CanvasGroup.alpha = 1;
        effectDictionary[type].Raycaster.enabled = true;
    }

    public void ShowLoadingIcon(float fadeTime = 1.5f)
    {
        loadingIcon.transform.DOKill();
        loadingIcon.transform.DOLocalRotate(new Vector3(0, 0, -rotateSpeed), 1, RotateMode.LocalAxisAdd).SetLoops(-1).SetEase(Ease.Linear);
        loadingIcon.DOKill();
        loadingIcon.DOFade(1, fadeTime);
    }

    public void HideLoadingIcon(float fadeTime = 1.5f)
    {
        loadingIcon.DOFade(0, fadeTime).OnComplete(() => loadingIcon.transform.DOKill());
    }

    public void FadeFromBlack(EffectType type, float fadeTime = 1.5f, System.Action onComplete = null)
    {
        StartCoroutine(FadeFromBlackRoutine(type, fadeTime, onComplete));
    }

    public IEnumerator FadeFromBlackRoutine(EffectType type, float fadeTime = 1.5f, System.Action onComplete = null)
    {
        effectDictionary[type].Image.color = Color.black;
        effectDictionary[type].Raycaster.enabled = true;
        yield return FadeOutRoutine(type, fadeTime, onComplete);
    }

    public void FadeToBlack(EffectType type, float fadeTime = 1.5f, System.Action onComplete = null)
    {
        StartCoroutine(FadeToBlackRoutine(type, fadeTime, onComplete));
    }

    public IEnumerator FadeToBlackRoutine(EffectType type, float fadeTime = 1.5f, System.Action onComplete = null)
    {
        effectDictionary[type].Image.color = Color.black;
        effectDictionary[type].Raycaster.enabled = true;
        yield return FadeInRoutine(type, fadeTime, onComplete);
    }

    public void FadeFromWhite(EffectType type, float fadeTime = 1.5f, System.Action onComplete = null)
    {
        StartCoroutine(FadeFromWhiteRoutine(type, fadeTime, onComplete));
    }

    public IEnumerator FadeFromWhiteRoutine(EffectType type, float fadeTime = 1.5f, System.Action onComplete = null)
    {
        effectDictionary[type].Image.color = Color.white;
        effectDictionary[type].Raycaster.enabled = true;
        yield return FadeOutRoutine(type, fadeTime, onComplete);
    }

    public void FadeToWhite(EffectType type, float fadeTime = 1.5f, System.Action onComplete = null)
    {
        StartCoroutine(FadeToWhiteRoutine(type, fadeTime, onComplete));
    }

    public IEnumerator FadeToWhiteRoutine(EffectType type, float fadeTime = 1.5f, System.Action onComplete = null)
    {
        effectDictionary[type].Image.color = Color.white;
        effectDictionary[type].Raycaster.enabled = true;
        yield return FadeInRoutine(type, fadeTime, onComplete);
    }

    IEnumerator FadeInRoutine(EffectType type, float fadeTime = 1.5f, System.Action onComplete = null)
    {
        effectDictionary[type].CanvasGroup.alpha = 0;
        effectDictionary[type].CanvasGroup.DOFade(1, fadeTime);

        yield return new WaitForSeconds(fadeTime);

        effectDictionary[type].Raycaster.enabled = true;
        onComplete?.Invoke();
    }

    IEnumerator FadeOutRoutine(EffectType type, float fadeTime = 1.5f, System.Action onComplete = null)
    {
        effectDictionary[type].CanvasGroup.DOFade(0, fadeTime);

        yield return new WaitForSeconds(fadeTime);

        effectDictionary[type].Raycaster.enabled = false;
        onComplete?.Invoke();
    }
}