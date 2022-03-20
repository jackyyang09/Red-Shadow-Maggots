using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GraphicsSettings : BasicSingleton<GraphicsSettings>
{
    [SerializeField] PostProcessLayer cameraPostProcess;
    public PostProcessLayer PostProcessing
    {
        get
        {
            return cameraPostProcess;
        }
    }

    int shadowLevel = 5;
    public int ShadowLevel
    {
        get { return Mathf.Min(QualitySettings.GetQualityLevel(), shadowLevel); }
    }
    public static System.Action<int> OnQualityLevelChanged;

    public void SetGraphicsLevel(float newLevel)
    {
        QualitySettings.SetQualityLevel((int)newLevel);
        OnQualityLevelChanged?.Invoke((int)newLevel);
    }

    [ContextMenu(nameof(DebugTest))]
    void DebugTest()
    {
        Debug.Log(QualitySettings.names[QualitySettings.GetQualityLevel()]);
    }
}