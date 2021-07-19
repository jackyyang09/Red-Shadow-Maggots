using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class QuickTimeResultText : MonoBehaviour
{
    [SerializeField] float textVerticalTween = 5;
    [SerializeField] float textFadeTime = 0.5f;
    [SerializeField] Vector2 positionRange = new Vector2();

    [SerializeField] GameObject[] resultTextPrefabs = null;
    [SerializeField] QuickTimeBase quickTimeEntity = null;

    private void OnEnable()
    {
        quickTimeEntity.OnQuickTimeComplete += OnQuickTimeComplete;
    }

    private void OnDisable()
    {
        quickTimeEntity.OnQuickTimeComplete -= OnQuickTimeComplete;
    }

    private void OnQuickTimeComplete(DamageStruct result)
    {
        StartCoroutine(AnimateText(result));
    }

    IEnumerator AnimateText(DamageStruct result)
    {
        RectTransform rect = transform as RectTransform;
        rect.anchoredPosition = new Vector2(
            Mathf.Lerp(positionRange.x, positionRange.y, result.barFill),
            rect.anchoredPosition.y);

        var text = Instantiate(resultTextPrefabs[(int)result.qteResult], transform).GetComponentInChildren<TMPro.TextMeshProUGUI>();
        RectTransform textRect = text.transform as RectTransform;

        text.DOFade(0, 0);
        text.DOFade(1, textFadeTime);

        textRect.DOAnchorPosY(textRect.anchoredPosition.y + textVerticalTween, textFadeTime);

        yield return new WaitForSeconds(1);

        text.DOFade(0, textFadeTime);

        Destroy(text);
    }
}