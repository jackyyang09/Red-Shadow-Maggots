using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseWorldProjector : MonoBehaviour
{
    [Tooltip("Create a RenderTexture of size (X, Y) with a Z depth")]
    [SerializeField] Vector3 renderTextureProps = new Vector3(512, 512, 24);
    [SerializeField] protected Camera cam;

    protected RenderTexture rt;
    public RenderTexture RenderTexture
    {
        get
        {
            if (rt == null)
            {
                rt = new RenderTexture((int)renderTextureProps.x, (int)renderTextureProps.y, (int)renderTextureProps.z);
            }
            if (!rt.IsCreated())
            {
                rt.Create();
            }
            return rt;
        }
    }

    private void OnDisable()
    {
        if (rt)
        {
            if (rt.IsCreated())
            {
                rt.Release();
            }
        }
    }

    protected virtual void Start()
    {
        cam.targetTexture = RenderTexture;
    }

    [ContextMenu(nameof(ReinitializeCamRT))]
    void ReinitializeCamRT()
    {
        rt.Release();
        rt = null;
        cam.targetTexture = RenderTexture;
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(GeneratePlaceholderGraphic))]
    void GeneratePlaceholderGraphic()
    {
        RenderTexture renderTexture = RenderTexture;

        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, true);
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
        tex.Apply();
        RenderTexture.active = null;

        Texture2D referenceTex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, true);
        referenceTex.SetPixels(tex.GetPixels());
        referenceTex.Apply();

        var data = ImageConversion.EncodeToPNG(referenceTex);

        var path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save Placeholder", "PlaceHolder", "png", "Save Image");
        if (string.IsNullOrEmpty(path)) return;

        System.IO.File.WriteAllBytes(path, data);

        UnityEditor.EditorUtility.DisplayDialog("File Saved", "File saved successfully!", "OK");

        UnityEditor.AssetDatabase.Refresh();

        UnityEditor.EditorGUIUtility.PingObject(UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Texture)));
    }
#endif
}