using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModularBox : MonoBehaviour
{
    [SerializeField]
    BoxCollider boxCollider;

    public Vector3 GetRandomPointInBox()
    {
        float x = Random.Range(0, boxCollider.size.x) - boxCollider.size.x / 2; 
        float y = Random.Range(0, boxCollider.size.y) - boxCollider.size.y / 2; 
        float z = Random.Range(0, boxCollider.size.z) - boxCollider.size.z / 2;
        return transform.position + new Vector3(x, y, z) + boxCollider.center;
    }

    public Vector3 GetBoxCenter()
    {
        return transform.position + boxCollider.center;
    }
}
