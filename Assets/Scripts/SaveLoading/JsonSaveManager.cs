using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonSaveManager<T> : BaseSaveManager<T> where T : new()
{
    public override string FilePath =>
    Path.Combine(System.Environment.CurrentDirectory,
        Application.persistentDataPath, Instance.FILE_NAME + ".json");

    public override void LoadData()
    {
        var text = File.ReadAllText(FilePath);
        if (string.IsNullOrEmpty(text))
        {
            CreateSave();
        }
        else
        {
            loadedData = JsonUtility.FromJson<T>(text);
            Debug.Log("Loaded " + FILE_NAME + " from file");
        }
    }

    public override void SaveData()
    {
        File.WriteAllText(FilePath, JsonUtility.ToJson(loadedData, true));
        Debug.Log("Data saved to " + FILE_NAME);
    }
}
