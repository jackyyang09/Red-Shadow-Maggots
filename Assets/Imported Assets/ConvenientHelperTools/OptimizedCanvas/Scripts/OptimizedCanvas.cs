using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class OptimizedCanvas : MonoBehaviour
{
    [SerializeField]
    Canvas canvas = null;

    [SerializeField]
    UnityEngine.UI.GraphicRaycaster caster;

    [SerializeField]
    OptimizedCanvas[] children;

    [SerializeField] public UnityEngine.Events.UnityEvent onCanvasShow;
    [SerializeField] public UnityEngine.Events.UnityEvent onCanvasHide;

    public bool IsVisible
    {
        get
        {
            return canvas.enabled;
        }
    }

    [ContextMenu("Find Get References")]
    private void OnValidate()
    {
        if (canvas != GetComponent<Canvas>()) canvas = GetComponent<Canvas>();
        if (caster != GetComponent<UnityEngine.UI.GraphicRaycaster>()) caster = GetComponent<UnityEngine.UI.GraphicRaycaster>();

        if (transform.childCount > 0)
        {
            LocateChildCanvases();
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Show Canvas")]
    public void EditorShow()
    {
        UnityEditor.Undo.RecordObject(canvas, "Modified canvas visibility");
        if (caster)
        {
            UnityEditor.Undo.RecordObject(caster, "Modified canvas visibility");
        }
        Show();
    }

    [ContextMenu("Hide Canvas")]
    public void EditorHide()
    {
        UnityEditor.Undo.RecordObject(canvas, "Modified canvas visibility");
        if (caster)
        {
            UnityEditor.Undo.RecordObject(caster, "Modified canvas visibility");
        }
        Hide();
    }

    [ContextMenu("Get References to Children")]
    public void FindChildCanvases()
    {
        UnityEditor.Undo.RecordObject(canvas, "Modified optimized canvas children");

        LocateChildCanvases();
    }
#endif

    public void LocateChildCanvases()
    {
        var childCanvases = GetComponentsInChildren<OptimizedCanvas>();
        if (children == null) return;
        bool differentSize = childCanvases.Length - 1 != children.Length;
        bool containsNull = false;
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == null)
            {
                containsNull = true;
                break;
            }
        }
        
        if (differentSize || containsNull)
        {
            children = new OptimizedCanvas[childCanvases.Length - 1];
            for (int i = 1; i < childCanvases.Length; i++)
            {
                children[i - 1] = childCanvases[i];
            }
        }
    }

    public void Show()
    {
        SetActive(true);
    }

    public void Hide()
    {
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        canvas.enabled = active;
        if (caster)
        {
            caster.enabled = active;
        }
        if (active)
        {
            if (children != null)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (active)
                    {
                        children[i].ShowIfPreviouslyVisible();
                    }
                }
            }
        }
        else
        {
            if (children != null)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].HideWithParent();
                }
            }
        }
        if (canvas.enabled) onCanvasShow.Invoke();
        else onCanvasHide.Invoke();
    }

    public void ShowIfPreviouslyVisible()
    {
        if (canvas.enabled)
        {
            if (caster)
            {
                caster.enabled = true;
            }
        }
        onCanvasShow.Invoke();
    }

    public void HideWithParent()
    {
        if (caster)
        {
            caster.enabled = false;
        }
    }
}
