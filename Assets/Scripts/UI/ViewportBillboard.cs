using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportBillboard : MonoBehaviour
{
    [SerializeField] public Vector3 offset;

    [SerializeField] RectTransform rect;

    [SerializeField] Camera cam;

    [SerializeField] Transform target;

    Vector3 originalScale;

    [ContextMenu("Find Rect")]
    private void OnValidate()
    {
        rect = GetComponent<RectTransform>();
    }

    public void EnableWithSettings(Camera newCam, Transform newTarget)
    {
        enabled = true;
        cam = newCam;
        target = newTarget;

        // Target doesn't know where to go until the next update, do this first
        originalScale = rect.localScale;
        rect.localScale = Vector3.zero;
    }

    private void Update()
    {
        Vector2 point = cam.WorldToViewportPoint(target.position + offset);
        //Vector2 point = cam.ScreenToViewportPoint(target.position);
        rect.anchoredPosition = point;
        rect.anchorMin = point;
        rect.anchorMax = point;
        rect.localScale = originalScale;
    }
}