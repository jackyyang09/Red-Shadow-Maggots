using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixClothBounds : MonoBehaviour
{
    [SerializeField]
    SkinnedMeshRenderer[] renderers;

    [SerializeField]
    Bounds realBounds = new Bounds();

    void OnRenderObject()
    {
        foreach (SkinnedMeshRenderer smr in renderers)
        {
            smr.localBounds = realBounds;
        }
    }
}