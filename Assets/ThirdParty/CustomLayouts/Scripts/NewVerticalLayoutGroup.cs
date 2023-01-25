using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewVerticalLayoutGroup : LayoutGroup
{
    [SerializeField] float spacing;

    List<AspectCorrectLayoutElement> children;
    Vector2[] childSizes;
    Vector2[] childPositions;

    public void FindChildren()
    {
        children = new List<AspectCorrectLayoutElement>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            if (!t.gameObject.activeSelf) continue;
            AspectCorrectLayoutElement e;
            if (transform.GetChild(i).TryGetComponent(out e))
            {
                children.Add(e);
            }
        }

        childSizes = new Vector2[children.Count];
        childPositions = new Vector2[children.Count];
    }

    public override void CalculateLayoutInputHorizontal()
    {
        FindChildren();

        for (int i = 0; i < children.Count; i++)
        {
            childSizes[i].x = 1;
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        float elementHeight = 1f / (float)children.Count;
        float spacingY = 0;
        for (int i = 0; i < children.Count; i++)
        {
            childPositions[i].y = spacingY;

            childSizes[i].x = elementHeight;
            spacingY += childSizes[i].y;
        }
    }

    public override void SetLayoutHorizontal()
    {
        for (int i = 0; i < children.Count; i++)
        {
            var rect = children[i].rectTransform;

            m_Tracker.Add(this, rect,
                DrivenTransformProperties.AnchoredPosition3D |
                DrivenTransformProperties.AnchorMin |
                DrivenTransformProperties.AnchorMax |
                DrivenTransformProperties.Pivot |
                DrivenTransformProperties.SizeDelta
                );

            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
        }
    }

    public override void SetLayoutVertical()
    {
        for (int i = 0; i < children.Count; i++)
        {
            var rect = children[i].rectTransform;

            rect.anchoredPosition = Vector2.Scale(childPositions[i], rectTransform.sizeDelta);
            rect.sizeDelta = Vector2.Scale(childSizes[i], rectTransform.sizeDelta);

            rect.anchoredPosition = Vector2.Scale(childPositions[i], rectTransform.sizeDelta);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, rectTransform.sizeDelta.x * childSizes[i].y);
        }
    }
}
