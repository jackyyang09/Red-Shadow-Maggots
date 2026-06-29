using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class BaseSaveManager<T> : Singleton<BaseSaveManager<T>> where T : new()
{
    public virtual string FILE_NAME { get { return "SAVEFILE"; } }
    public abstract string FilePath { get; }

    protected T loadedData;
    public T LoadedData { get { return loadedData; } }

    public static bool Initialized;

    protected virtual void Start()
    {
        LoadOrCreate();

        Initialized = true;
    }

    public void CreateSave()
    {
        Debug.Log(FilePath + " not found, creating a new instance");
        loadedData = new T();
    }

    public void LoadOrCreate()
    {
        if (File.Exists(FilePath))
        {
            LoadData();
        }
        else
        {
            CreateSave();
        }
    }

    public abstract void LoadData();

    public abstract void SaveData();

    public void ResetData()
    {
        loadedData = new T();
    }

    public void DeleteData()
    {
        if (File.Exists(FilePath))
        {
            File.WriteAllText(FilePath, string.Empty);

            Debug.Log("Deleted save data at " + FILE_NAME);
        }
    }
}