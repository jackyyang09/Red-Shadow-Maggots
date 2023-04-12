using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseCanvasProjection : MonoBehaviour
{
    [SerializeField] protected RawImage rawImage;
    [SerializeField] protected GameObject projectorPrefab;
    protected BaseWorldProjector worldProjector;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Initialize();
    }

    protected void Initialize()
    {
        worldProjector = Instantiate(projectorPrefab).GetComponent<BaseWorldProjector>();
        rawImage.texture = worldProjector.RenderTexture;
    }
}