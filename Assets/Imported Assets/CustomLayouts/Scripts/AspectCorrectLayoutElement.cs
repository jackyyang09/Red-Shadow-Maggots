using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AspectCorrectLayoutElement : MonoBehaviour
{
    public Vector2 aspectRatio = Vector2.one;

    public RectTransform rectTransform { get { return transform as RectTransform; } }

    private void OnEnable()
    {
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    private void OnDisable()
    {
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    private void OnRectTransformDimensionsChange()
    {
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }
#endif

}