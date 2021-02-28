using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [System.Serializable]
    public struct SaveData
    {
        public List<CharacterCard> cards;
    }

    [SerializeField] int[] party = new int[3] { 0, 1, 2 };
    public int[] Party
    {
        get
        {
            return party;
        }
    }

    [SerializeField] string saveFileName = "SaveData";
    [SerializeField] SaveData saveData = new SaveData();

    public List<CharacterCard> Cards
    {
        get
        {
            return saveData.cards;
        }
    }

    public string SaveFilePath
    {
        get
        {
            return Application.dataPath + "/" + saveFileName + ".json";
        }
    }

    public static SaveManager instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    private void OnEnable()
    {
        LoadFromJSON();
    }

    private void OnDisable()
    {
        SaveToJSON();
    }

    [ContextMenu("Debug Save Data")]
    public void SaveToJSON()
    {
        System.IO.File.WriteAllText(SaveFilePath, JsonUtility.ToJson(saveData));
        Debug.Log("Saved to " + SaveFilePath);
    }

    [ContextMenu("Debug Load Data")]
    public void LoadFromJSON()
    {
        if (!System.IO.File.Exists(SaveFilePath))
        {
            Debug.Log("No save file found! Creating a new one...");
            party = new int[] { 0, 1, 2 };
            saveData = new SaveData()
            {
                cards = new List<CharacterCard>()
                {
                    new CharacterCard()
                    {
                        characterID = 2, rarity = 0, time = 0
                    },
                    new CharacterCard()
                    {
                        characterID = 3, rarity = 0, time = 0
                    },
                    new CharacterCard()
                    {
                        characterID = 1, rarity = 0, time = 0
                    }
                }
            };
        }
        else
        {
            Debug.Log("Loaded from " + SaveFilePath);
            saveData = JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(SaveFilePath));
        }
    }

    void EstablishSingletonDominance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // A unique case where the Singleton exists but not in this scene
            if (instance.gameObject.scene.name == null)
            {
                instance = this;
            }
            else if (!instance.gameObject.activeInHierarchy)
            {
                instance = this;
            }
            else if (instance.gameObject.scene.name != gameObject.scene.name)
            {
                instance = this;
            }
            Destroy(gameObject);
        }
    }
}
