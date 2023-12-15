using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class RoundChangeGraphic : MonoBehaviour
{
    [SerializeField] float specialFontSize = 55;
    [SerializeField] float tweenTime = 1;

    [SerializeField] TextMeshProUGUI label;

    RectTransform rectTransform => transform as RectTransform;

    public void ShowForRound(int x)
    {
        gameObject.SetActive(true);
        label.text = "Round\n<size=" + specialFontSize + ">" + x + "</size>";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
