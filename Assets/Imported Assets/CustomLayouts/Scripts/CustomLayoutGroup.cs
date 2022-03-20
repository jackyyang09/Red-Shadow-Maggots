using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CustomLayoutGroup))]
public class CustomLayoutGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        DrawPropertiesExcluding(serializedObject, new string[] 
        {
            "m_Script", "m_Padding", "m_ChildAlignment"
        });
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

/// <summary>
/// https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/UIAutoLayout.html#understanding-layout-controllers
/// </summary>
public class CustomLayoutGroup : LayoutGroup
{
    enum LayoutMode
    {
        Uniform,
        Proportional
    }

    enum LayoutDirection
    {
        Horizontal,
        Vertical
    }

    [Tooltip("Uniform - Space is divided between layout elements evenly\n" +
        "Proportional - Space relative to size of layout area is given to elements relative to their width/height value")]
    [SerializeField] LayoutMode mode;
    [SerializeField] LayoutDirection direction;

    [Tooltip("If true, will try to expand elements width-wise to fill all empty space")]
    [SerializeField] bool expandWidth;
    [Tooltip("If true, will try to expand elements height-wise to fill all empty space")]
    [SerializeField] bool expandHeight;

    [SerializeField] Vector2 spacing;

    List<CustomLayoutElement> children = new List<CustomLayoutElement>();
    Vector2[] childPositions;
    Vector2[] childSizes;

    public void RefreshLayout()
    {
        switch (direction)
        {
            case LayoutDirection.Horizontal:
                SetLayoutHorizontal();
                break;
            case LayoutDirection.Vertical:
                SetLayoutVertical();
                break;
        }
    }

    void SetChildrenProperties()
    {
        for (int i = 0; i < children.Count; i++)
        {
            var rect = children[i].GetRectTransform();
            rect.anchoredPosition = childPositions[i];
            rect.sizeDelta = childSizes[i];
        }
    }

    public override void SetLayoutHorizontal()
    {
        SetChildrenProperties();
    }

    public override void SetLayoutVertical()
    {
        SetChildrenProperties();
    }

    public Vector2 EvaluateAnchor(CustomLayoutElement element, float width, float height)
    {
        var anchor = element.Anchor;
        var anchoredPosition = element.rectTransform.anchoredPosition;
        var sizeDelta = element.rectTransform.sizeDelta;
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                break;
            case TextAnchor.UpperCenter:
                anchoredPosition = new Vector2(anchoredPosition.x + width / 2 - sizeDelta.x / 2, anchoredPosition.y);
                break;
            case TextAnchor.UpperRight:
                anchoredPosition = new Vector2(anchoredPosition.x + width - sizeDelta.x, anchoredPosition.y);
                break;
            case TextAnchor.MiddleLeft:
                anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y - height / 2 + sizeDelta.y / 2);
                break;
            case TextAnchor.MiddleCenter:
                anchoredPosition = new Vector2(anchoredPosition.x + width / 2 - sizeDelta.x / 2, anchoredPosition.y - height / 2 + sizeDelta.y / 2);
                break;
            case TextAnchor.MiddleRight:
                anchoredPosition = new Vector2(anchoredPosition.x + width - sizeDelta.x, anchoredPosition.y - height / 2 + sizeDelta.y / 2);
                break;
            case TextAnchor.LowerLeft:
                anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y - height + sizeDelta.y);
                break;
            case TextAnchor.LowerCenter:
                anchoredPosition = new Vector2(anchoredPosition.x + width / 2 - sizeDelta.x / 2, anchoredPosition.y - height + sizeDelta.y);
                break;
            case TextAnchor.LowerRight:
                anchoredPosition = new Vector2(anchoredPosition.x + width - sizeDelta.x, anchoredPosition.y - height + sizeDelta.y);
                break;
        }
        return anchoredPosition;
    }

    public override void CalculateLayoutInputVertical()
    {
        if (direction != LayoutDirection.Horizontal) return;

        children = new List<CustomLayoutElement>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            if (!t.gameObject.activeSelf) continue;
            CustomLayoutElement e;
            if (transform.GetChild(i).TryGetComponent(out e))
            {
                children.Add(e);
            }
        }

        float spaceX = 0;
        float rectWidth = rectTransform.rect.size.x;
        float layoutWidth = rectTransform.rect.size.x / children.Count;

        childPositions = new Vector2[children.Count];
        childSizes = new Vector2[children.Count];

        for (int i = 0; i < children.Count; i++)
        {
            var r = children[i].rectTransform;
#if UNITY_EDITOR
            m_Tracker.Add(this, r,
                DrivenTransformProperties.AnchoredPosition3D |
                DrivenTransformProperties.AnchorMin |
                DrivenTransformProperties.AnchorMax |
                DrivenTransformProperties.Pivot |
                DrivenTransformProperties.SizeDelta
                );
#endif

            // Force anchors for easier calculations
            children[i].rectTransform.anchoredPosition = Vector2.zero;
            children[i].rectTransform.anchorMin = new Vector2(0, 1);
            children[i].rectTransform.anchorMax = new Vector2(0, 1);
            children[i].rectTransform.pivot = new Vector2(0, 1);

            if (expandWidth)
            {
                float newWidth = rectWidth * children[i].Width;
                r.sizeDelta = new Vector2(newWidth, r.sizeDelta.y);
            }
            if (expandHeight)
            {
                r.sizeDelta = new Vector2(r.sizeDelta.x, rectTransform.rect.size.y);
            }

            float leftSpace = i == 0 ? 0 : spacing.x / 2;
            float rightSpace = i == children.Count - 1 ? 0 : spacing.x / 2;

            float elementWidth = 0;
            switch (mode)
            {
                case LayoutMode.Uniform:
                    elementWidth = layoutWidth - spacing.x;

                    r.sizeDelta = new Vector2(elementWidth * children[i].Width, Mathf.Min(r.sizeDelta.y, rectTransform.rect.size.y) * children[i].Height);

                    children[i].PostProcessSize(new Vector2(elementWidth, rectTransform.rect.size.y));
                    spaceX += leftSpace;
                    r.anchoredPosition = new Vector2(spaceX, 0) + EvaluateAnchor(children[i], layoutWidth, rectTransform.rect.size.y);
                    spaceX += elementWidth + rightSpace;
                    break;
                case LayoutMode.Proportional:
                    elementWidth = rectTransform.rect.size.x * children[i].Width;

                    r.sizeDelta = new Vector2(elementWidth, r.sizeDelta.y * children[i].Height);
                    children[i].PostProcessSize(r.sizeDelta);
                    spaceX += leftSpace;
                    r.anchoredPosition = new Vector2(spaceX + spacing.x * i, 0) + EvaluateAnchor(children[i], elementWidth, rectTransform.rect.size.y);
                    spaceX += elementWidth + rightSpace;
                    break;
            }
            children[i].PostProcessPosition();
            childPositions[i] = children[i].GetRectTransform().anchoredPosition;
            childSizes[i] = children[i].GetRectTransform().sizeDelta;
        }
    }

    public override void CalculateLayoutInputHorizontal()
    {
        if (direction != LayoutDirection.Vertical) return;

        children = new List<CustomLayoutElement>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            if (!t.gameObject.activeSelf) continue;
            CustomLayoutElement e;
            if (t.TryGetComponent(out e))
            {
                children.Add(e);
            }
        }

        float spaceY = 0;
        float layoutHeight = rectTransform.rect.size.y / children.Count;

        childPositions = new Vector2[children.Count];
        childSizes = new Vector2[children.Count];

        for (int i = 0; i < children.Count; i++)
        {
            var r = children[i].rectTransform;
#if UNITY_EDITOR
            m_Tracker.Add(this, r,
                DrivenTransformProperties.AnchoredPosition3D |
                DrivenTransformProperties.AnchorMin |
                DrivenTransformProperties.AnchorMax |
                DrivenTransformProperties.Pivot |
                DrivenTransformProperties.SizeDelta
                );
#endif

            // Force anchors for easier calculations
            children[i].rectTransform.anchoredPosition = Vector2.zero;
            children[i].rectTransform.anchorMin = new Vector2(0, 1);
            children[i].rectTransform.anchorMax = new Vector2(0, 1);
            children[i].rectTransform.pivot = new Vector2(0, 1);

            if (expandWidth)
            {
                r.sizeDelta = new Vector2(rectTransform.rect.size.x, r.sizeDelta.y);
            }
            if (expandHeight)
            {
                float newHeight = rectTransform.rect.size.y * children[i].Height;
                r.sizeDelta = new Vector2(r.sizeDelta.x, newHeight);
            }

            float topSpace = i == 0 ? 0 : spacing.y / 2;
            float bottomSpace = i == children.Count - 1 ? 0 : spacing.y / 2;

            float elementHeight = 0;
            switch (mode)
            {
                case LayoutMode.Uniform:
                    elementHeight = layoutHeight - spacing.y;

                    r.sizeDelta = new Vector2(Mathf.Min(r.sizeDelta.x, rectTransform.rect.size.x) * children[i].Width, elementHeight * children[i].Height);

                    children[i].PostProcessSize(new Vector2(r.sizeDelta.x, elementHeight));
                    spaceY -= topSpace;
                    r.anchoredPosition = new Vector2(0, spaceY) + EvaluateAnchor(children[i], rectTransform.rect.size.x, layoutHeight);
                    spaceY -= elementHeight + bottomSpace;
                    break;
                case LayoutMode.Proportional:
                    elementHeight = rectTransform.rect.size.y * children[i].Height;

                    r.sizeDelta = new Vector2(Mathf.Min(r.sizeDelta.x, rectTransform.rect.size.x) * children[i].Width, elementHeight);
                    children[i].PostProcessSize(r.sizeDelta);
                    spaceY -= topSpace;
                    r.anchoredPosition = new Vector2(0, spaceY - spacing.y * i) + EvaluateAnchor(children[i], rectTransform.rect.size.x, elementHeight);
                    spaceY -= elementHeight + bottomSpace;
                    break;
            }
            children[i].PostProcessPosition();
            childPositions[i] = children[i].GetRectTransform().anchoredPosition;
            childSizes[i] = children[i].GetRectTransform().sizeDelta;
        }
    }
}