using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OptimizedCanvas))]
[RequireComponent(typeof(Canvas))]
public class OptimizedButton : MonoBehaviour
{
    public OptimizedCanvas canvas;

    [SerializeField] Button button;

    [ContextMenu("Find References")]
    private void OnValidate()
    {
        if (canvas != GetComponent<OptimizedCanvas>()) canvas = GetComponent<OptimizedCanvas>();
        if (button != GetComponent<Button>()) button = GetComponent<Button>();
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

    public void SetButtonInteractable(bool b)
    {
        button.interactable = b;
    }

    public void SetActive(bool active)
    {
        canvas.SetActive(active);
    }
}
