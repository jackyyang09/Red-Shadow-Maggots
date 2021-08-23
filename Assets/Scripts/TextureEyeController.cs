using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureEyeController : MonoBehaviour
{
    public enum EyeLookMode
    { 
        Both,
        Separate
    }

    public EyeLookMode eyeLookMode = EyeLookMode.Both;

    [SerializeField] int leftEyeMaterialIndex = 3;
    [SerializeField] int rightEyeMaterialIndex = 2;

    [Header("Eye Properties")]
    public Vector3 leftEyePos = new Vector3();
    public Vector3 LeftEyeWorldPosition { get { return head.TransformPoint(leftEyePos); } }
    public Vector3 rightEyePos = new Vector3();
    public Vector3 RightEyeWorldPosition { get { return head.TransformPoint(rightEyePos); } }

    [SerializeField] float maxOffset = 0.5f;

    [Header("Object References")]
    [SerializeField] new Renderer renderer;
    [SerializeField] Transform head = null;
    public Vector3 ToHeadSpace(Vector3 pos) => head.InverseTransformPoint(pos);

    public Transform lookTarget = null;
    public Transform leftTarget = null;
    public Transform rightTarget = null;

    Material leftEyeMaterial, rightEyeMaterial;

#if UNITY_EDITOR
    [Header("Editor Properties")]
    [SerializeField] bool drawEyeGizmos = true;
    [SerializeField] bool editorTimeMaterialInstances = true;
    [SerializeField] float eyeWorldScale = 0.2f;
    private void OnDrawGizmosSelected()
    {
        Vector3 direction = new Vector3();
        {
            Vector3 eyePos = LeftEyeWorldPosition;
            
            switch (eyeLookMode)
            {
                case EyeLookMode.Both:
                    direction = (lookTarget.position - eyePos).normalized;
                    break;
                case EyeLookMode.Separate:
                    direction = (leftTarget.position - eyePos).normalized;
                    break;
            }
            if (drawEyeGizmos)
            {
                Gizmos.DrawSphere(eyePos, eyeWorldScale);
                Gizmos.DrawLine(eyePos, eyePos + direction);
            }
        }

        {
            Vector3 eyePos = RightEyeWorldPosition;
            
            switch (eyeLookMode)
            {
                case EyeLookMode.Both:
                    direction = (lookTarget.position - eyePos).normalized;
                    break;
                case EyeLookMode.Separate:
                    direction = (rightTarget.position - eyePos).normalized;
                    break;
            }
            if (drawEyeGizmos)
            {
                Gizmos.DrawSphere(eyePos, eyeWorldScale);
                Gizmos.DrawLine(eyePos, eyePos + direction);
            }
        }
    }
#endif

    private void Awake()
    {
        GetMaterialReferences();
    }

    private void Update()
    {
        UpdateEyeLookDirection();
    }

    /// <summary>
    /// This is what we call, a "Pro Gamer Move"
    /// </summary>
    public void CreateMaterialInstancesInEditor()
    {
        Resources.UnloadUnusedAssets();

        // Calling renderer.materials will throw some errors, lets ignore those
        if (leftEyeMaterial == null) leftEyeMaterial = renderer.materials[leftEyeMaterialIndex];
        if (rightEyeMaterial == null) rightEyeMaterial = renderer.materials[rightEyeMaterialIndex];
    }

    public void AnimationCallback()
    {
        if (leftEyeMaterial == null) leftEyeMaterial = renderer.materials[leftEyeMaterialIndex];
        if (rightEyeMaterial == null) rightEyeMaterial = renderer.materials[rightEyeMaterialIndex];
        UpdateEyeLookDirection();
    }

    public void GetMaterialReferences()
    {
        leftEyeMaterial = renderer.materials[leftEyeMaterialIndex];
        rightEyeMaterial = renderer.materials[rightEyeMaterialIndex];
    }

    public void UpdateEyeLookDirection()
    {
        Vector3 direction = new Vector3();
        {
            Vector3 eyePos = LeftEyeWorldPosition;
            switch (eyeLookMode)
            {
                case EyeLookMode.Both:
                    direction = (lookTarget.position - eyePos).normalized;
                    break;
                case EyeLookMode.Separate:
                    direction = (leftTarget.position - eyePos).normalized;
                    break;
            }
            float eyeAngleX = Vector3.Angle(-head.transform.right, direction);
            float eyeAngleY = Vector3.Angle(-head.transform.up, direction);
            Vector2 eyeAngle = new Vector2(Mathf.InverseLerp(0, 179, eyeAngleX), Mathf.InverseLerp(0, 179, eyeAngleY));
            Vector2 textureOffset = new Vector2(Mathf.Lerp(-maxOffset, maxOffset, eyeAngle.x), Mathf.Lerp(-maxOffset, maxOffset, eyeAngle.y));
            leftEyeMaterial.mainTextureOffset = textureOffset;
        }
        {
            Vector3 eyePos = RightEyeWorldPosition;
            switch (eyeLookMode)
            {
                case EyeLookMode.Both:
                    direction = (lookTarget.position - eyePos).normalized;
                    break;
                case EyeLookMode.Separate:
                    direction = (rightTarget.position - eyePos).normalized;
                    break;
            }
            float eyeAngleX = Vector3.Angle(-head.transform.right, direction);
            float eyeAngleY = Vector3.Angle(-head.transform.up, direction);
            Vector2 eyeAngle = new Vector2(Mathf.InverseLerp(0, 179, eyeAngleX), Mathf.InverseLerp(0, 179, eyeAngleY));
            Vector2 textureOffset = new Vector2(Mathf.Lerp(-maxOffset, maxOffset, eyeAngle.x), Mathf.Lerp(-maxOffset, maxOffset, eyeAngle.y));
            rightEyeMaterial.mainTextureOffset = textureOffset;
        }
    }

    private void Reset()
    {
        renderer = GetComponentInChildren<Renderer>();
    }
}
