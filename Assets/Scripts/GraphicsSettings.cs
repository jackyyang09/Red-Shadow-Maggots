using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsSettings : BasicSingleton<GraphicsSettings>
{
    int shadowLevel = 5;
    public int ShadowLevel
    {
        get { return Mathf.Min(QualitySettings.GetQualityLevel(), shadowLevel); }
    }
    public static System.Action<int> OnQualityLevelChanged;

    [ContextMenu(nameof(DebugTest))]
    void DebugTest()
    {
        Debug.Log(QualitySettings.names[QualitySettings.GetQualityLevel()]);
    }
}