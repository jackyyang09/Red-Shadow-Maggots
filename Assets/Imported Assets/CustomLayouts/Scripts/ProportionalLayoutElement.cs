using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProportionalLayoutElement : MonoBehaviour
{
    [SerializeField] TextAnchor anchor = TextAnchor.UpperLeft;
    public TextAnchor Anchor { get { return anchor; } }

    [Tooltip("The relative width-wise space this element will occupy in it's given area")]
    [SerializeField] float width = 1;
    public float Width { get { return width; } }

    [Tooltip("The relative height-wise space this element will occupy in it's given area")]
    [SerializeField] float height = 1;
    public float Height { get { return height; } }

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
