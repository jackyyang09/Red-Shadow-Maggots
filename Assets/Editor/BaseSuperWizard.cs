using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class BaseSuperWizard<AssetType, WindowType> : EditorWindow 
    where AssetType : ScriptableObject
    where WindowType : BaseSuperWizard<AssetType, WindowType>
{
    // Source
    // https://answers.unity.com/questions/403782/find-instance-of-editorwindow-without-creating-new.html
    public static WindowType FindFirstInstance()
    {
        var windows = (WindowType[])Resources.FindObjectsOfTypeAll(typeof(WindowType));
        if (windows.Length == 0)
            return null;
        return windows[0];
    }

    protected static WindowType window;
    public static WindowType Window
    {
        get
        {
            if (!window)
            {
                window = FindFirstInstance();
                if (window == null)
                {
                    window = GetWindow<WindowType>();
                    window.titleContent = new GUIContent(Window.WindowName);
                }
            }
            return window;
        }
    }

    public static bool IsOpen => window != null;

    protected virtual string AssetPath => "Assets/Editor/";
    protected virtual string AssetName => nameof(AssetType);
    protected virtual string AssetFileName => AssetName + ".asset";
    protected string FullAssetPath => AssetPath + AssetFileName;

    protected AssetType data;
    protected AssetType Data
    {
        get
        {
            if (!data)
            {
                data = AssetDatabase.LoadAssetAtPath<AssetType>(FullAssetPath);
                if (data == null)
                {
                    Window.LogToIntegratedConsole("No " + AssetName + " found! " +
                        "Creating a new asset at " +
                        FullAssetPath);
                    var so = CreateInstance<AssetType>();
                    AssetDatabase.CreateAsset(so, FullAssetPath);
                    data = so;
                }
                else
                {
                    Window.LogToIntegratedConsole("Successfully loaded SaveCreationData");
                }
            }
            return data;
        }
    }

    static SerializedObject so;
    protected static SerializedObject SerializedObject
    {
        get
        {
            if (so == null)
            {
                so = new SerializedObject(Window.Data);
                Window.DesignateSerializedProperties();
            }
            return so;
        }
    }

    protected virtual string WindowName => typeof(AssetType).Name;

    string internalConsole;

    //[MenuItem("Base Super Wizard")]
    //static void Init()
    //{
    //    if (!Window.propertiesDesignated) Window.DesignateSerializedProperties();
    //}

    // OnEnable happens before Init??
    protected virtual void OnEnable()
    {
        if (!propertiesDesignated) DesignateSerializedProperties();
    }

    protected virtual void OnDisable()
    {

    }

    bool propertiesDesignated;
    protected bool PropertiesDesignated => propertiesDesignated;

    protected virtual void DesignateSerializedProperties()
    {
        propertiesDesignated = true;
    }

    protected void LogToIntegratedConsole(string log)
    {
        string time = System.DateTime.Now.TimeOfDay.ToString(@"hh\:mm\:ss");
        internalConsole = "[" + time + "] " + log + "\n" + internalConsole;
    }

    protected SerializedProperty FindProp(string prop) => SerializedObject.FindProperty(prop);
}