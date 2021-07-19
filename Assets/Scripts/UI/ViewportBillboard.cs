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
    [SerializeField] RectTransform canvas = null;

    [ContextMenu("Find Rect")]
    private void OnValidate()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<UnityEngine.UI.CanvasScaler>().transform as RectTransform;
        }
    }

    public void EnableWithSettings(Camera newCam, Transform newTarget)
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<UnityEngine.UI.CanvasScaler>().transform as RectTransform;
        }
        enabled = true;
        cam = newCam;
        target = newTarget;

        // Target doesn't know where to go until the next update, do this first
        originalScale = rect.localScale;
        //rect.localScale = Vector3.zero;

        UpdatePosition();
    }

    private void Update()
    {
        UpdatePosition();   
    }

    void UpdatePosition()
    {
        Vector2 point = cam.WorldToViewportPoint(target.position + offset);
        float width = canvas.sizeDelta.x / 2;
        float height = canvas.sizeDelta.y / 2;
        point = new Vector3(Mathf.Lerp(-width, width, point.x), Mathf.Lerp(-height, height, point.y));
        rect.anchoredPosition = point;
    }
}