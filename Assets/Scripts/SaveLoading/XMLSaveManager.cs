using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public abstract class XMLSaveManager<T> : BaseSaveManager<T> where T : new()
{
    public override string FilePath => 
        Path.Combine(System.Environment.CurrentDirectory, 
            Application.persistentDataPath, Instance.FILE_NAME + ".xml");

    public override void LoadData()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        StreamReader reader = new StreamReader(FilePath);
        loadedData = (T)serializer.Deserialize(reader.BaseStream);
        reader.Close();
        Debug.Log("Loaded " + FILE_NAME + " from file");
    }

    public override void SaveData()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        StreamWriter writer = new StreamWriter(FilePath);
        serializer.Serialize(writer.BaseStream, loadedData);
        writer.Close();

        Debug.Log("Saved " + FILE_NAME + " to file");
    }
}