using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIElementCenterer : MonoBehaviour
{
    [SerializeField] RectTransform parentRect = null;

    [SerializeField] List<RectTransform> rects = null;
    [SerializeField] List<TextMeshProUGUI> textComponents = null;

    [SerializeField] float characterWidth = 25;
    [SerializeField] float padding = 10;

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    [ContextMenu("Recenter")]
    public void Recenter()
    {
        for (int i = 0; i < textComponents.Count; i++)
        {
            float requiredWidth = textComponents[i].text.Length * characterWidth;
            textComponents[i].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, requiredWidth);
        }

        float totalWidth = padding * rects.Count;
        foreach (RectTransform rect in rects)
        {
            totalWidth += rect.rect.width;
        }

        float halfWidth = totalWidth / 2;
        float occupiedWidth = parentRect.rect.width / 2;
        for (int i = 0; i < rects.Count; i++)
        {
            rects[i].anchoredPosition = new Vector2(occupiedWidth - halfWidth, rects[i].anchoredPosition.y);
            occupiedWidth += padding + rects[i].rect.width;
        }
    }
}
