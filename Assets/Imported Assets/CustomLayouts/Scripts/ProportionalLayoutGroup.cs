using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ProportionalLayoutGroup))]
public class ProportionalLayoutGroupEditor : Editor
{
    SerializedProperty layoutMode;

    private void OnEnable()
    {
        layoutMode = serializedObject.FindProperty(nameof(layoutMode));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(layoutMode);

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }
}
#endif

public class ProportionalLayoutGroup : LayoutGroup
{
    enum LayoutMode
    {
        Uniform,
        Proportional
    }

    [SerializeField] LayoutMode layoutMode = LayoutMode.Uniform;

    [SerializeField] float spacing;

    List<ProportionalLayoutElement> children;
    Vector2[] childSizes;
    Vector2[] childPositions;

    public void FindChildren()
    {
        children = new List<ProportionalLayoutElement>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            if (!t.gameObject.activeSelf) continue;
            ProportionalLayoutElement e;
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

        float elementWidth = 1f / (float)children.Count;
        float spacingX = 0;
        switch (layoutMode)
        {
            case LayoutMode.Uniform:
                for (int i = 0; i < children.Count; i++)
                {
                    childPositions[i].x = spacingX;

                    childSizes[i].x = elementWidth * children[i].Width;
                    spacingX += elementWidth;
                }
                break;
            case LayoutMode.Proportional:
                for (int i = 0; i < children.Count; i++)
                {
                    childPositions[i].x = spacingX;

                    childSizes[i].x = elementWidth * children[i].Width;
                    spacingX += childSizes[i].x;
                }
                break;
        }
        
    }

    public override void CalculateLayoutInputVertical()
    {
        for (int i = 0; i < children.Count; i++)
        {
            childPositions[i].y = 0;

            childSizes[i].y = children[i].Height;
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

            rect.anchoredPosition = Vector2.Scale(childPositions[i], rectTransform.sizeDelta);

            rect.sizeDelta = Vector2.Scale(childSizes[i], rectTransform.sizeDelta);
        }
    }

    public override void SetLayoutVertical()
    {
        float elementWidth = rectTransform.sizeDelta.x / (float)children.Count;
        float elementHeight = rectTransform.sizeDelta.y;
        for (int i = 0; i < children.Count; i++)
        {
            var rect = children[i].rectTransform;

            rect.anchoredPosition = Vector2.Scale(childPositions[i], rectTransform.sizeDelta);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, rectTransform.sizeDelta.y * childSizes[i].y);

            EvaluateAnchor(children[i], elementWidth, elementHeight);
        }
    }

    public void EvaluateAnchor(ProportionalLayoutElement element, float width, float height)
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
}
