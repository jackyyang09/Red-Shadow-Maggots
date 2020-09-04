using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OptimizedCanvas))]
[RequireComponent(typeof(Canvas))]
public class OptimizedButton : MonoBehaviour
{
    [SerializeField]
    OptimizedCanvas canvas;

    [ContextMenu("Find References")]
    private void OnValidate()
    {
        if (canvas != GetComponent<OptimizedCanvas>()) canvas = GetComponent<OptimizedCanvas>();
    }

    [ContextMenu("Show Button")]
    public void Show()
    {
        SetActive(true);
    }

    [ContextMenu("Hide Button")]
    public void Hide()
    {
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        canvas.SetActive(active);
    }
}
