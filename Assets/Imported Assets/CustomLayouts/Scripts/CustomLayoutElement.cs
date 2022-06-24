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
        get { return aspectRatio; } 
    }

    [SerializeField] bool aspectCorrect;
    public bool AspectCorrect { get { return aspectCorrect; } }

    [SerializeField] bool clampToParentSize;
    public bool ClampToParentSize { get { return clampToParentSize; } }

    [SerializeField] Vector2 positionalOffset;
    public Vector2 PositionalOffset { get { return positionalOffset; } }

    public RectTransform rectTransform { get { return transform as RectTransform; } }

    public void OverrideData(ResolutionSpecificElementProperties.ResolutionDataPair dataPair)
    {
        width = dataPair.widthOverride;
        height = dataPair.heightOverride;
        positionalOffset = dataPair.positionalOffset;
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

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
