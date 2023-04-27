using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

public abstract class BaseSaveManager<T> : Singleton<BaseSaveManager<T>> where T : new()
{
    public virtual string FILE_NAME { get { return "SAVEFILE"; } }
    public static string FILE_PATH
    { 
        get 
        { 
            return Path.Combine(System.Environment.CurrentDirectory, 
                Application.persistentDataPath, Instance.FILE_NAME + ".xml");
        }
    }

    protected T loadedData;
    public T LoadedData { get { return loadedData; } }

    public static bool Initialized;

    protected virtual void Start()
    {
        LoadData();

        Initialized = true;
    }

    public void LoadData()
    {
        if (File.Exists(FILE_PATH))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StreamReader reader = new StreamReader(FILE_PATH);
            loadedData = (T)serializer.Deserialize(reader.BaseStream);
            reader.Close();
            Debug.Log("Loaded " + FILE_NAME + " from file");
        }
        else
        {
            Debug.Log(FILE_PATH + " not found, creating a new instance");
            loadedData = new T();
        }
    }
    
    public void SaveData()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        StreamWriter writer = new StreamWriter(FILE_PATH);
        serializer.Serialize(writer.BaseStream, loadedData);
        writer.Close();
    
        Debug.Log("Saved " + FILE_NAME + " to file");
    }

    public void ResetData()
    {
        loadedData = new T();
    }

    public void DeleteData()
    {
        if (File.Exists(FILE_PATH))
        {
            File.Delete(FILE_PATH);

            Debug.Log("Deleted save data at " + FILE_NAME);
        }
    }
}