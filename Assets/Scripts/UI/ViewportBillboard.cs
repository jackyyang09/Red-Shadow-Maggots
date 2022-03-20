using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportBillboard : MonoBehaviour
{
    [SerializeField] float hideDistance = 0;
    [SerializeField] float fadeDistance = 1;
    [SerializeField] Vector2 scale = new Vector2(0.7f, 1.25f);
    [SerializeField] Vector2 scaleDistance = new Vector2(5, 20);

    [SerializeField] public Vector3 offset;

    [SerializeField] Transform target;
    public Vector3 TargetWithOffset { get { return target.position + offset; } }

    [SerializeField] RectTransform rect;

    [SerializeField] Camera cam;

    Vector3 originalScale;
    [SerializeField] RectTransform canvas = null;

    [SerializeField] CanvasGroup canvasGroup = null;

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

    private void FixedUpdate()
    {
        UpdatePosition();

        if (cam.CanSeePoint(TargetWithOffset))
        {
            float distance = Vector3.Distance(target.position, cam.transform.position);

            //if (canvasGroup)
            //{
            //    if (distance < hideDistance)
            //    {
            //        canvasGroup.alpha = 0;
            //    }
            //    else
            //    {
            //        float lerp = Mathf.InverseLerp(hideDistance, hideDistance + fadeDistance, distance);
            //        canvasGroup.alpha = Mathf.Lerp(0, 1, lerp);
            //    }
            //}

            transform.localScale = Vector3.one * Mathf.Lerp(scale.x, scale.y, Mathf.InverseLerp(scaleDistance.x, scaleDistance.y, distance));
        }
        else
        {
            transform.localScale = Vector3.zero;
        }
    }

    void UpdatePosition()
    {
        Vector2 point = cam.WorldToViewportPoint(TargetWithOffset);
        float width = canvas.sizeDelta.x / 2;
        float height = canvas.sizeDelta.y / 2;
        point = new Vector3(Mathf.Lerp(-width, width, point.x), Mathf.Lerp(-height, height, point.y));
        rect.anchoredPosition = point;
    }
}