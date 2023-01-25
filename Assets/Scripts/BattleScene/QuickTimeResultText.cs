using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class QuickTimeResultText : MonoBehaviour
{
    [SerializeField] float textVerticalTween = 5;
    [SerializeField] float textTweenDelay = 0.5f;
    [SerializeField] float textTweenTime = 0.5f;
    [SerializeField] float textFadeTime = 0.5f;
    [SerializeField] Vector2 positionRange = new Vector2();

    [SerializeField] GameObject[] resultTextPrefabs = null;
    [SerializeField] QuickTimeBase quickTimeEntity = null;

    private void OnEnable()
    {
        quickTimeEntity.OnExecuteQuickTime += OnExecuteQuickTime;
    }

    private void OnDisable()
    {
        quickTimeEntity.OnExecuteQuickTime -= OnExecuteQuickTime;
    }

    private void OnExecuteQuickTime()
    {
        StartCoroutine(AnimateText(BaseCharacter.IncomingDamage));
    }

    IEnumerator AnimateText(DamageStruct result)
    {
        RectTransform rect = transform as RectTransform;
        rect.anchoredPosition = new Vector2(
            Mathf.Lerp(positionRange.x, positionRange.y, result.barFill),
            rect.anchoredPosition.y);

        var text = Instantiate(resultTextPrefabs[(int)result.qteResult], transform).GetComponentInChildren<TMPro.TextMeshProUGUI>();
        RectTransform textRect = text.rectTransform;

        if (result.qteResult == QuickTimeBase.QTEResult.Perfect)
        {
            textRect.DOPunchScale(Vector3.one * 1.25f, 0.5f, 0, 0).SetEase(Ease.OutCubic);
        }

        text.DOFade(0, 0);
        text.DOFade(1, textFadeTime);

        yield return new WaitForSeconds(textTweenDelay);

        textRect.DOAnchorPosY(textRect.anchoredPosition.y + textVerticalTween, textTweenTime);

        yield return new WaitForSeconds(textTweenTime);

        text.DOFade(0, textFadeTime);

        yield return new WaitForSeconds(textFadeTime);

        Destroy(text);
    }
}