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
    [SerializeField] Vector2 shadowOpacity = new Vector2(0, 200);

    private void Start()
    {
        //if (graphicsSettings.ShadowLevel == 1)
        //{
        //    enabled = false;
        //}

        for (int i = 0; i < defaultRenderers.Length; i++)
        {
            defaultRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    private void Update()
    {
        Ray ray = new Ray();
        ray.direction = Vector3.down;
        ray.origin = meshRoot.position - new Vector3(0, distanceOffset, 0);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            transform.position = hit.point + Vector3.up * shadowOffset;
            transform.forward = Vector3.up;
            renderer.color = new Color32(0, 0, 0, (byte)Mathf.Lerp(shadowOpacity.y, shadowOpacity.x, hit.distance / maxDistance));
        }
        renderer.enabled = hit.collider;
    }
}