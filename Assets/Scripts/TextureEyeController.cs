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

    [SerializeField] int leftEyeMatIndex = -1;
    [SerializeField] int rightEyeMatIndex = -1;

    [Header("Eye Properties")]
    public Vector3 leftEyePos = new Vector3();
    public Vector3 LeftEyeWorldPosition { get { return head.TransformPoint(leftEyePos); } }
    public Vector3 rightEyePos = new Vector3();
    public Vector3 RightEyeWorldPosition { get { return head.TransformPoint(rightEyePos); } }

    [Tooltip("Useful if you modified your material to have smaller/larger eyes than normal. Applied additively")]
    [SerializeField] Vector2 standardOffset = new Vector2();
    [SerializeField] float maxOffset = 0.5f;

    [Header("Object References")]
    [SerializeField] new Renderer renderer;
    [SerializeField] Transform head = null;
    public Vector3 ToHeadSpace(Vector3 pos) => head.InverseTransformPoint(pos);

    public Transform lookTarget = null;
    public Transform leftTarget = null;
    public Transform rightTarget = null;

    Material leftEyeMaterialRef, rightEyeMaterialRef;

    [SerializeField, HideInInspector] Material[] backupMaterials;

#if UNITY_EDITOR
    [Header("Editor Properties")]
    [SerializeField] bool drawEyeGizmos = true;
    [SerializeField] float eyeWorldScale = 0.2f;
    [SerializeField, HideInInspector] bool editorTimeEyes = true;
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

    /// <summary>
    /// This is what we call, a "Pro Gamer Move"
    /// </summary>
    public bool CreateMaterialInstancesInEditor()
    {
        Resources.UnloadUnusedAssets();

        if (renderer.sharedMaterials.Length == 0) return false;

        Debug.Log("Enabled Editor Texture Eyes");
 
        bool tainted = false;
        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
        {
            if (UnityEditor.AssetDatabase.GetAssetPath(renderer.sharedMaterials[i]).Equals(""))
            {
                tainted = true;
                break;
            }
        }
        if (!tainted)
        {
            backupMaterials = renderer.sharedMaterials;
        }

        AssignMaterialReferences();

        editorTimeEyes = true;

        return true;
    }

    public void UnloadEditorEyes()
    {
        leftEyeMaterialRef = null;
        rightEyeMaterialRef = null;

        editorTimeEyes = false;

        if (backupMaterials.Length == 0) return;

        renderer.sharedMaterials = backupMaterials;

        Resources.UnloadUnusedAssets();
    }
#endif

    private void Awake()
    {
        AssignMaterialReferences();
    }

    private void Update()
    {
        UpdateEyeLookDirection();
    }

    public void AnimationCallback()
    {
        if (!editorTimeEyes) return;
        AssignMaterialReferences();
        UpdateEyeLookDirection();
    }

    public void AssignMaterialReferences()
    {
        // Calling renderer.materials will throw some errors, lets ignore those
        if (!Application.isPlaying)
        {
            var newMaterials = renderer.sharedMaterials;
            if (leftEyeMatIndex > 0)
            {
                leftEyeMaterialRef = new Material(renderer.sharedMaterials[leftEyeMatIndex]);
                newMaterials[leftEyeMatIndex] = leftEyeMaterialRef;
            }
            if (rightEyeMatIndex > 0)
            {
                rightEyeMaterialRef = new Material(renderer.sharedMaterials[leftEyeMatIndex]);
                newMaterials[rightEyeMatIndex] = rightEyeMaterialRef;
            }
            renderer.sharedMaterials = newMaterials;
        }
        else
        {
            if (leftEyeMatIndex > 0)
            {
                leftEyeMaterialRef = renderer.materials[leftEyeMatIndex];
            }
            if (rightEyeMatIndex > 0)
            {
                rightEyeMaterialRef = renderer.materials[rightEyeMatIndex];
            }
        }
    }

    public void UpdateEyeLookDirection()
    {
        Vector3 direction = new Vector3();
        if (leftEyeMaterialRef != null)
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
            leftEyeMaterialRef.mainTextureOffset = standardOffset + textureOffset;
        }

        if (rightEyeMaterialRef != null)
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
            rightEyeMaterialRef.mainTextureOffset = standardOffset + textureOffset;
        }
    }

    private void Reset()
    {
        renderer = GetComponentInChildren<Renderer>();
    }
}
