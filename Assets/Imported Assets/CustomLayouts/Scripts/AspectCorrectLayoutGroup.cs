using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(AspectCorrectLayoutGroup))]
public class AspectCorrectLayoutGroupEditor : Editor
{
    SerializedProperty direction;

    private void OnEnable()
    {
        direction = serializedObject.FindProperty(nameof(direction));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(direction);

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }
}
#endif

public class AspectCorrectLayoutGroup : LayoutGroup
{
    enum Direction 
    {
        Horizontal,
        Vertical
    }

    [SerializeField] float spacing;
    [SerializeField] Direction direction;

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

        switch (direction)
        {
            case Direction.Horizontal:
                float elementWidth = 1f / (float)children.Count;
                float spacingX = 0;
                for (int i = 0; i < children.Count; i++)
                {
                    childPositions[i].x = spacingX;

                    childSizes[i].x = elementWidth;
                    spacingX += childSizes[i].x;
                }
                break;
            case Direction.Vertical:
                float elementHeight = 1f / (float)children.Count;
                float spacingY = 0;
                for (int i = 0; i < children.Count; i++)
                {
                    childPositions[i].y = spacingY;

                    childSizes[i].y = elementHeight;

                    float ar = children[i].aspectRatio.x / children[i].aspectRatio.y;
                    childSizes[i].x = elementHeight * ar;

                    spacingY -= childSizes[i].y;
                }
                break;
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        switch (direction)
        {
            case Direction.Horizontal:
                for (int i = 0; i < children.Count; i++)
                {
                    childPositions[i].y = 0;

                    float ar = children[i].aspectRatio.x / children[i].aspectRatio.y;

                    childSizes[i].y = childSizes[i].x / ar;
                }
                break;
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
        for (int i = 0; i < children.Count; i++)
        {
            var rect = children[i].rectTransform;

            rect.anchoredPosition = Vector2.Scale(childPositions[i], rectTransform.sizeDelta);
            rect.sizeDelta = Vector2.Scale(childSizes[i], rectTransform.sizeDelta);
        }
    }
}
