using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CustomLayoutElement : LayoutElement
{
    [SerializeField] TextAnchor anchor = TextAnchor.UpperLeft;
    public TextAnchor Anchor => anchor;

    [Tooltip("The relative width-wise space this element will occupy in its given area")]
    [SerializeField] float width = 1;
    public float Width => width;

    [Tooltip("The relative height-wise space this element will occupy in its given area")]
    [SerializeField] float height = 1;
    public float Height => height;

    [Header("Post-Layout Adjustments")]
    [SerializeField] Vector2 aspectRatio = new Vector2(16, 9);
    public Vector2 AspectRatio
    {
        set
        {
            aspectRatio = value;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        get => aspectRatio;
    }

    [SerializeField] bool aspectCorrect;
    public bool AspectCorrect => aspectCorrect;

    [SerializeField] bool clampToParentSize;
    public bool ClampToParentSize => clampToParentSize;

    [SerializeField] Vector2 positionalOffset;
    public Vector2 PositionalOffset => positionalOffset;

    public RectTransform rectTransform => transform as RectTransform;

    public CustomLayoutGroup ParentGroup;
    public float WidthBudget;
    public float HeightBudget;

    public void OverrideData(ResolutionSpecificElementProperties.ResolutionDataPair dataPair)
    {
        width = dataPair.widthOverride;
        height = dataPair.heightOverride;
        positionalOffset = dataPair.positionalOffset;
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    protected override void OnEnable()
    {
        //base.OnEnable();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    protected override void OnDisable()
    {
        //base.OnDisable();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    protected override void OnRectTransformDimensionsChange()
    {
        //base.OnRectTransformDimensionsChange();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    public override void CalculateLayoutInputHorizontal()
    {
        Debug.Log(nameof(CalculateLayoutInputHorizontal));
        if (!ParentGroup) return;
        preferredWidth = WidthBudget * ParentGroup.TrueSizeDelta.x;
        var s = rectTransform.sizeDelta;
        s.x = preferredWidth;
        rectTransform.sizeDelta = s;
    }

    public override void CalculateLayoutInputVertical()
    {
        Debug.Log(nameof(CalculateLayoutInputVertical));
        switch (ParentGroup.Direction)
        {
            case CustomLayoutGroup.LayoutDirection.Horizontal:
                if (aspectCorrect)
                {
                    preferredHeight = HeightBudget;
                }
                else
                {
                    preferredHeight = HeightBudget * ParentGroup.TrueSizeDelta.y;
                }
                break;
            case CustomLayoutGroup.LayoutDirection.Vertical:
                preferredHeight = HeightBudget;
                break;
        }
        var s = rectTransform.sizeDelta;
        s.y = preferredHeight;
        rectTransform.sizeDelta = s;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(CustomLayoutElement))]
public class CustomLayoutElementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
#endif