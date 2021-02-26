using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    SuperRare,
    UltraRare,
    Count
}

public class GachaSystem : MonoBehaviour
{
    [SerializeField] System.DateTime timeTest;

    [Range(0, 1)]
    [SerializeField] float chanceOfCommon = 0;

    [Range(0, 1)]
    [SerializeField] float chanceOfRare = 0;

    [Range(0, 1)]
    [SerializeField] float chanceOfSuperRare = 0;

    int dudCount = 0;

    [SerializeField] List<CharacterObject> maggots = null;

    public static GachaSystem instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    void Start()
    {
        BattleSystem.instance.SpawnCharacterWithRarity(GetRandomMaggot(), GetRandomRarity());
        BattleSystem.instance.SpawnCharacterWithRarity(GetRandomMaggot(), GetRandomRarity());
        BattleSystem.instance.SpawnCharacterWithRarity(GetRandomMaggot(), GetRandomRarity());

        BattleSystem.instance.GameStart();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public CharacterObject GetRandomMaggot()
    {
        var maggot = maggots[Random.Range(0, maggots.Count)];
        maggots.Remove(maggot);
        return maggot;
    }

    [ContextMenu("RNG Test")]
    public Rarity GetRandomRarity()
    {
        // TODO: Create a pool of cookies that the player owns

        float rng = Random.value;
        if (rng > chanceOfRare)
        {
            dudCount++;
            if (dudCount == 3)
                return Rarity.Rare;
            else
                return Rarity.Common;
        }
        else if (rng > chanceOfSuperRare && rng <= chanceOfRare)
        {
            return Rarity.Rare;
        }
        //else if (rng <= chanceOfSuperRare)
        //{
        return Rarity.SuperRare;
        //}
    }

    [ContextMenu("Time Test")]
    public void TestTime()
    {
        Debug.Log(System.DateTime.Now.Ticks);
    }


    [ContextMenu("Normalize Weights")]
    void NormalizeValues()
    {
        float weights = chanceOfCommon + chanceOfRare + chanceOfSuperRare;
        chanceOfCommon *= weights;
        chanceOfRare *= weights;
        chanceOfSuperRare *= weights;
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
