using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericBillboard : MonoBehaviour
{
    [Header("Leave null for Camera")]

    [SerializeField]
    Transform target;

    public static Camera cam;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    public void Start()
    {
        if (target == null) target = cam.transform;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.LookAt(target);
    }
}
