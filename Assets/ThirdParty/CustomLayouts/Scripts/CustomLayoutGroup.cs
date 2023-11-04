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

    public enum LayoutDirection
    {
        Horizontal,
        Vertical
    }

    enum SpacingMode
    {
        Classic,
        Individual
    }

    [Tooltip("Uniform - Space is divided between layout elements evenly\n" +
        "Proportional - Space relative to size of layout area is given to elements relative to their width/height value")]
    [SerializeField] LayoutMode mode;
    [SerializeField] LayoutDirection direction;
    public LayoutDirection Direction => direction;

    [SerializeField] SpacingMode spacingMode = SpacingMode.Classic;
    [SerializeField] Vector2 spacing;

    List<CustomLayoutElement> children = new List<CustomLayoutElement>();
    Vector2[] childPositions;

    // Cache this value
    Vector2 trueSizeDelta
    {
        get
        {
            var canvas = transform.root.GetComponentInChildren<Canvas>();

            var rect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);

            return rect.size;
        }
    }
    public Vector2 TrueSizeDelta { get; private set; }
    
    [ContextMenu(nameof(GetTrueSizeDelta))]
    void GetTrueSizeDelta()
    {
        Debug.Log(trueSizeDelta);
    }

    public override void CalculateLayoutInputHorizontal()
    {
        Debug.Log(nameof(CalculateLayoutInputHorizontal));
        FindChildren();

        if (children.Count == 0) return;

        float spaceX = 0;

        float elementWidth = 1f / children.Count;

        switch (direction)
        {
            case LayoutDirection.Horizontal:
                for (int i = 0; i < children.Count; i++)
                {
                    switch (mode)
                    {
                        case LayoutMode.Uniform:
                            childPositions[i].x = spaceX;
                            children[i].WidthBudget = elementWidth * children[i].Width;
                            Debug.Log(children[i].Width);
                            //childSizes[i].x = elementWidth * children[i].Width;
                            spaceX += elementWidth;
                            break;
                        case LayoutMode.Proportional:
                            childPositions[i].x = spaceX;
                            children[i].WidthBudget = children[i].Width;
                            //childSizes[i].x = children[i].Width;
                            spaceX += children[i].Width;
                            break;
                    }
                }
                break;
            case LayoutDirection.Vertical:
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].WidthBudget = children[i].Width;
                    //childSizes[i].x = children[i].Width;
                }
                break;
        }
    }

    public override void SetLayoutHorizontal()
    {
        Debug.Log(nameof(SetLayoutHorizontal));
        TrueSizeDelta = trueSizeDelta;
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
            rect.anchoredPosition = Vector2.Scale(childPositions[i], TrueSizeDelta);
            //rect.sizeDelta = Vector2.Scale(childSizes[i], trueSizeDelta);
            //var size = Vector2.Scale(childSizes[i], trueSize);
            //Debug.Log(childSizes[i] + " " + trueSize + " " + size);
            //children[i].preferredWidth = size.x;
            //children[i].preferredHeight = size.y;
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        Debug.Log(nameof(CalculateLayoutInputVertical));
        if (children.Count == 0) return;

        switch (direction)
        {
            case LayoutDirection.Horizontal:
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].AspectCorrect)
                    {
                        float ar = children[i].AspectRatio.x / children[i].AspectRatio.y;

                        children[i].HeightBudget = children[i].rectTransform.sizeDelta.x / ar;
                        //childSizes[i].y = children[i].rectTransform.sizeDelta.x / ar;
                    }
                    else
                    {
                        children[i].HeightBudget = children[i].Height;
                        //childSizes[i].y = children[i].Height;
                    }
                }
                break;
        }
    }

    public override void SetLayoutVertical()
    {
        Debug.Log(nameof(SetLayoutVertical));
        switch (direction)
        {
            case LayoutDirection.Vertical:
                float spaceY = 0;
                float elementHeight = TrueSizeDelta.y / children.Count;
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].AspectCorrect)
                    {
                        float ar = children[i].AspectRatio.x / children[i].AspectRatio.y;

                        childPositions[i].y = spaceY;
                        children[i].HeightBudget = children[i].rectTransform.sizeDelta.x / ar;
                        //childSizes[i].y = children[i].rectTransform.sizeDelta.x / ar;
                        spaceY -= children[i].HeightBudget;
                    }
                    else
                    {
                        switch (mode)
                        {
                            case LayoutMode.Uniform:
                                childPositions[i].y = spaceY;
                                children[i].HeightBudget = elementHeight * children[i].Height;
                                //childSizes[i].y = elementHeight * children[i].Height;
                                spaceY -= elementHeight;
                                break;
                            case LayoutMode.Proportional:
                                childPositions[i].y = spaceY;
                                children[i].HeightBudget = trueSizeDelta.y * children[i].Height;
                                spaceY -= children[i].HeightBudget;
                                break;
                        }
                    }
                }
                break;
        }

        for (int i = 0; i < children.Count; i++)
        {
            var rect = children[i].rectTransform;

            float width = 0;
            float height = 0;
            switch (direction)
            {
                case LayoutDirection.Horizontal:
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, childPositions[i].y * TrueSizeDelta.y);

                    //if (children[i].AspectCorrect)
                    //{
                    //    children[i].preferredHeight = childSizes[i].y;
                    //    Debug.Log(children[i].preferredHeight);
                    //}
                    //else
                    //{
                    //    children[i].preferredHeight = childSizes[i].y * trueSizeDelta.y;
                    //    Debug.Log(children[i].preferredHeight);
                    //}

                    switch (mode)
                    {
                        case LayoutMode.Uniform:
                            width = TrueSizeDelta.x / children.Count;
                            break;
                        case LayoutMode.Proportional:
                            width = rect.sizeDelta.x;
                            break;
                    }
                    height = TrueSizeDelta.y;
                    break;
                case LayoutDirection.Vertical:
                    if (children[i].AspectCorrect)
                    {
                        //rect.sizeDelta = new Vector2(rect.sizeDelta.x, childSizes[i].y);
                        //children[i].preferredWidth = rect.sizeDelta.x;
                        //children[i].preferredHeight = childSizes[i].y;

                        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, childPositions[i].y);
                    }
                    else
                    {
                        //rect.sizeDelta = new Vector2(rect.sizeDelta.x, childSizes[i].y);
                        //children[i].preferredWidth = rect.sizeDelta.x;
                        //children[i].preferredHeight = childSizes[i].y;

                        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, childPositions[i].y);
                    }

                    width = trueSizeDelta.x;
                    switch (mode)
                    {
                        case LayoutMode.Uniform:
                            height = trueSizeDelta.y / children.Count;
                            break;
                        case LayoutMode.Proportional:
                            height = rect.sizeDelta.y;
                            break;
                    }
                    break;
            }

            EvaluateAnchor(children[i], width, height);

            rect.anchoredPosition += children[i].PositionalOffset;
        }
    }

    public void FindChildren()
    {
        children = new List<CustomLayoutElement>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            if (!t.gameObject.activeSelf) continue;
            if (transform.GetChild(i).TryGetComponent(out CustomLayoutElement e))
            {
                e.ParentGroup = this;
                children.Add(e);
            }
        }
        childPositions = new Vector2[children.Count];
    }

    public void EvaluateAnchor(CustomLayoutElement element, float width, float height)
    {
        var pos = element.rectTransform.anchoredPosition;
        var size = element.rectTransform.sizeDelta;
        switch (element.Anchor)
        {
            case TextAnchor.UpperLeft:
                break;
            case TextAnchor.UpperCenter:
                pos = new Vector2(pos.x + width / 2 - size.x / 2, pos.y);
                break;
            case TextAnchor.UpperRight:
                pos = new Vector2(pos.x + width - size.x, pos.y);   
                break;
            case TextAnchor.MiddleLeft:
                pos = new Vector2(pos.x, pos.y - height / 2 + size.y / 2);
                break;
            case TextAnchor.MiddleCenter:
                pos = new Vector2(pos.x + width / 2 - size.x / 2, pos.y - height / 2 + size.y / 2);
                break;
            case TextAnchor.MiddleRight:
                pos = new Vector2(pos.x + width - size.x, pos.y - height / 2 + size.y / 2);
                break;
            case TextAnchor.LowerLeft:
                pos = new Vector2(pos.x, pos.y - height + size.y);
                break;
            case TextAnchor.LowerCenter:
                pos = new Vector2(pos.x + width / 2 - size.x / 2, pos.y - height + size.y);
                break;
            case TextAnchor.LowerRight:
                pos = new Vector2(pos.x + width - size.x, pos.y - height + size.y);
                break;
        }
        element.rectTransform.anchoredPosition = pos;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }
#endif
}
