using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JarateHealth : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer jarRenderer = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        jarRenderer.GetBlendShapeWeight(0);
    }
}
