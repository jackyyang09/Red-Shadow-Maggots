using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportBillboard : MonoBehaviour
{
    [SerializeField] public Vector3 offset;

    [SerializeField] RectTransform rect;

    [SerializeField] Camera cam;

    [SerializeField] Transform target;

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
        Update();
    }

    private void Update()
    {
        Vector2 point = cam.WorldToViewportPoint(target.position + offset);
        //Vector2 point = cam.ScreenToViewportPoint(target.position);
        rect.anchoredPosition = point;
        rect.anchorMin = point;
        rect.anchorMax = point;
    }
}