using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CustomLayoutElement : MonoBehaviour
{
    [SerializeField] TextAnchor anchor = TextAnchor.UpperLeft;
    public TextAnchor Anchor { get { return anchor; } }

    [Tooltip("The relative width-wise space this element will occupy in it's given area")]
    [SerializeField] float width = 1;
    public float Width { get { return width; } }

    [Tooltip("The relative height-wise space this element will occupy in it's given area")]
    [SerializeField] float height = 1;
    public float Height { get { return height; } }

    [Header("Post-Layout Adjustments")]
    [SerializeField] Vector2 aspectRatio = new Vector2(16, 9);
    public Vector2 AspectRatio
    {
        set
        {
            aspectRatio = value;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }

    [SerializeField] bool matchHeightToWidth;
    [SerializeField] bool matchWidthToHeight;

    [SerializeField] Vector2 positionalOffset;

    public RectTransform rectTransform { get { return transform as RectTransform; } }
    Vector2 sizeDelta { get { return rectTransform.sizeDelta; } set { rectTransform.sizeDelta = value; } }

    private void OnEnable()
    {
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    private void OnDisable()
    {
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }
#endif

    public void PostProcessSize(Vector2 elementSize)
    {
        float ar = aspectRatio.x / aspectRatio.y;
        if (matchHeightToWidth)
        {
            var x = Mathf.Min(sizeDelta.x, elementSize.x);
            sizeDelta = new Vector2(x, x / ar);
            if (sizeDelta.y > elementSize.y)
            {
                sizeDelta = new Vector2(elementSize.y * ar, elementSize.y);
            }
        }
        if (matchWidthToHeight)
        {
            var y = Mathf.Min(sizeDelta.y, elementSize.y);
            sizeDelta = new Vector2(y * ar, y);
        }
    }

    public void PostProcessPosition()
    {
        rectTransform.anchoredPosition += positionalOffset;
    }
}
