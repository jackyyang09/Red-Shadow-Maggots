﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class OptimizedCanvas : MonoBehaviour
{
    [SerializeField] bool hideOnAwake = false;

    [SerializeField] Canvas canvas = null;

    public RectTransform rectTransform
    {
        get
        {
            return transform as RectTransform;
        }
    }

    [SerializeField] GraphicRaycaster caster;

    [SerializeField] OptimizedCanvas[] children = new OptimizedCanvas[0];
    [SerializeField] List<LayoutGroup> layoutGroups = null;
    [SerializeField] List<LayoutElement> layoutElements = null;

    [SerializeField] public UnityEngine.Events.UnityEvent onCanvasShow;
    [SerializeField] public UnityEngine.Events.UnityEvent onCanvasHide;

    Coroutine layoutRoutine = null;

    public bool IsVisible
    {
        get
        {
            return canvas.enabled;
        }
    }

    void Awake()
    {
        if (hideOnAwake)
        {
            Hide();
        }
    }

    [ContextMenu("Find Get References")]
    private void OnValidate()
    {
        if (canvas != GetComponent<Canvas>()) canvas = GetComponent<Canvas>();
        if (caster != GetComponent<GraphicRaycaster>()) caster = GetComponent<GraphicRaycaster>();

        if (transform.childCount > 0)
        {
            LocateChildCanvases();
        }
        LocateLayoutComponents();
    }

#if UNITY_EDITOR
    public void RegisterUndo()
    {
        UnityEditor.Undo.RecordObject(canvas, "Modified canvas visibility");
        if (caster)
        {
            UnityEditor.Undo.RecordObject(caster, "Modified canvas visibility");
        }
    }

    [ContextMenu("Show Canvas")]
    public void EditorButtonShow()
    {
        RegisterUndo();
        Show();

        List<GameObject> selected = new List<GameObject>(UnityEditor.Selection.gameObjects);
        for (int i = 0; i < selected.Count; i++)
        {
            if (selected[i] == gameObject) continue;
            OptimizedCanvas selectedCanvas = null;
            if (selected[i].TryGetComponent(out selectedCanvas))
            {
                selectedCanvas.RegisterUndo();
                selectedCanvas.Show();
            }
        }
    }

    [ContextMenu("Hide Canvas")]
    public void EditorHide()
    {
        RegisterUndo();
        Hide();
        
        var selected = UnityEditor.Selection.gameObjects;
        for (int i = 0; i < selected.Length; i++)
        {
            if (selected[i] == gameObject) continue;
            OptimizedCanvas selectedCanvas = null;
            if (selected[i].TryGetComponent(out selectedCanvas))
            {
                selectedCanvas.RegisterUndo();
                selectedCanvas.Hide();
            }
        }
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

        bool containsNull = false;

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == null)
            {
                containsNull = true;
                break;
            }
        }
        
        if (containsNull)
        {
            children = new OptimizedCanvas[childCanvases.Length - 1];
            for (int i = 1; i < childCanvases.Length; i++)
            {
                children[i - 1] = childCanvases[i];
            }
        }
    }

    public void LocateLayoutComponents()
    {
        layoutElements = new List<LayoutElement>(GetComponentsInChildren<LayoutElement>());
        layoutGroups = new List<LayoutGroup>(GetComponentsInChildren<LayoutGroup>());

        for (int i = layoutElements.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < children.Length; j++)
            {
                if (layoutElements[i].transform.IsChildOf(children[j].transform))
                {
                    layoutElements.RemoveAt(i);
                    break;
                }
            }
        }

        for (int i = layoutGroups.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < children.Length; j++)
            {
                if (layoutGroups[i].transform.IsChildOf(children[j].transform))
                {
                    layoutGroups.RemoveAt(i);
                    break;
                }
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
        if (canvas == null) return;
        canvas.enabled = active;
        if (caster)
        {
            caster.enabled = active;
        }
        if (children != null)
        {
            if (active)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (active)
                    {
                        children[i].ShowIfPreviouslyVisible();
                    }
                }
            }
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].HideWithParent();
                }
            }
        }

        if (Application.isPlaying)
        {
            if (layoutRoutine != null) StopCoroutine(layoutRoutine);
            layoutRoutine = StartCoroutine(FlashLayoutComponents(active));
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

    IEnumerator FlashLayoutComponents(bool showForOneFrame)
    {
        if (showForOneFrame)
        {
            for (int i = 0; i < layoutElements.Count; i++)
            {
                layoutElements[i].enabled = true;
            }

            for (int i = 0; i < layoutGroups.Count; i++)
            {
                layoutGroups[i].enabled = true;
            }

            yield return new WaitForEndOfFrame();
        }
        
        for (int i = 0; i < layoutElements.Count; i++)
        {
            layoutElements[i].enabled = false;
        }

        for (int i = 0; i < layoutGroups.Count; i++)
        {
            layoutGroups[i].enabled = false;
        }

        layoutRoutine = null;
    }
}
