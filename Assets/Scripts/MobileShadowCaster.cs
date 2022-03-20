using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class MobileShadowCaster : MonoBehaviour
{
    [SerializeField] Renderer[] defaultRenderers = null;

    [SerializeField] Transform meshRoot;
    [SerializeField] float distanceOffset = 0;
    [SerializeField] float shadowOffset = 0.01f;
    [SerializeField] float maxDistance = 5;
    [SerializeField] LayerMask layerMask;
    [SerializeField] new SpriteRenderer renderer;
    //[SerializeField] new Renderer renderer;
    Material material;
    int colorProp;
    [SerializeField] Vector2 shadowOpacity = new Vector2(0, 200);

#if UNITY_ANDROID
    private void Start()
    {
        //if (graphicsSettings.ShadowLevel > 1)
        //{
        //    gameObject.SetActive(false);
        //    return;
        //}

        for (int i = 0; i < defaultRenderers.Length; i++)
        {
            defaultRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        material = renderer.material;
        colorProp = Shader.PropertyToID("_Color");
    }

    private void Update()
    {
        Ray ray = new Ray();
        ray.direction = Vector3.down;
        ray.origin = meshRoot.position;
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit, maxDistance + distanceOffset, layerMask))
        {
            transform.position = hit.point + Vector3.up * shadowOffset;
            transform.forward = Vector3.up;
            float distance = hit.distance - distanceOffset;
            byte alpha = (byte)Mathf.Lerp(shadowOpacity.y, shadowOpacity.x, distance / maxDistance);
            Color newColor = new Color32(0, 0, 0, alpha);
            renderer.color = newColor;
            //material.SetColor(colorProp, newColor);
        }
        renderer.enabled = hit.collider;
    }
#endif
}