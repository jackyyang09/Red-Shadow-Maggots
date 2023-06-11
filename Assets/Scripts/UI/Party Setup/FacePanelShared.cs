using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FacePanelShared : MonoBehaviour
{
    [field: SerializeField] public float MinDragDistance { get; private set; }
    [field: SerializeField] public float MinHoldTime { get; private set; }
    [field: SerializeField] public OptimizedCanvas Canvas { get; private set; }
    [field: SerializeField] public Collider2D Collider { get; private set; }
    [field: SerializeField] public Image ProfileGraphic { get; private set; }
    [field: SerializeField] public Image ClassGraphic { get; private set; }
    [field: SerializeField] public Image HealthBar { get; private set; }
    [field: SerializeField] public CanvasGroup InPartyCG { get; private set; }
}
