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

    [ContextMenu("Find Get References")]
    private void OnValidate()
    {
        if (canvas != GetComponent<Canvas>()) canvas = GetComponent<Canvas>();
        if (caster == null) caster = GetComponent<UnityEngine.UI.GraphicRaycaster>();
    }

    [ContextMenu("Show Canvas")]
    public void Show()
    {
        SetActive(true);
    }

    [ContextMenu("Hide Canvas")]
    public void Hide()
    {
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        canvas.enabled = active;
        if (caster) caster.enabled = active;

        foreach (OptimizedCanvas canvas in children)
        {
            canvas.SetActive(active);
        }
    }
}
