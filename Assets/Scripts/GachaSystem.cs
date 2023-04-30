using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Facade;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    SuperRare,
    UltraRare,
    Count
}

public class GachaSystem : BasicSingleton<GachaSystem>
{
    [SerializeField] bool legacyMode;
    public bool LegacyMode => legacyMode;

    [Range(0, 1)]
    [SerializeField] float chanceOfCommon = 0;

    [Range(0, 1)]
    [SerializeField] float chanceOfUncommon = 0;

    [Range(0, 1)]
    [SerializeField] float chanceOfRare = 0;

    [Range(0, 1)]
    [SerializeField] float chanceOfSuperRare = 0;

    [Range(0, 1)]
    [SerializeField] float chanceOfUltraRare = 0;

    int dudCount = 0;

    [SerializeField] List<AssetReference> maggotReferences;
    public AssetReference RandomMaggot { get { return maggotReferences[Random.Range(0, maggotReferences.Count)]; } }
    public List<AsyncOperationHandle<CharacterObject>> LoadedMaggots = new List<AsyncOperationHandle<CharacterObject>>();
    public void TryAddLoadedMaggot(AsyncOperationHandle<CharacterObject> maggot)
    {
        if (!LoadedMaggots.Contains(maggot)) LoadedMaggots.Add(maggot);
    }

    [SerializeField] List<CharacterObject> maggots = null;

    [Header("Legacy")]
    List<CharacterObject> offenseMaggots = new List<CharacterObject>();
    List<CharacterObject> defenseMaggots = new List<CharacterObject>();
    List<CharacterObject> supportMaggots = new List<CharacterObject>();

    // Start is called before the first frame update
    void Start()
    {
        if (legacyMode)
        {
            for (int i = 0; i < maggots.Count; i++)
            {
                switch (maggots[i].characterClass)
                {
                    case CharacterClass.Offense:
                        offenseMaggots.Add(maggots[i]);
                        break;
                    case CharacterClass.Defense:
                        defenseMaggots.Add(maggots[i]);
                        break;
                    case CharacterClass.Support:
                        supportMaggots.Add(maggots[i]);
                        break;
                }
            }

            if (offenseMaggots.Count > 0)
                characterLoader.SpawnCharacterWithRarity(0, GetRandomMaggot(offenseMaggots), RandomRarity);
            if (defenseMaggots.Count > 0)
                characterLoader.SpawnCharacterWithRarity(1, GetRandomMaggot(defenseMaggots), RandomRarity);
            if (supportMaggots.Count > 0)
                characterLoader.SpawnCharacterWithRarity(2, GetRandomMaggot(supportMaggots), RandomRarity);
        }
    }

    public CharacterObject GetRandomMaggot(List<CharacterObject> maggotList)
    {
        var maggot = maggotList[Random.Range(0, maggotList.Count)];
        maggotList.Remove(maggot);
        return maggot;
    }

    private void OnCharacterLoaded(AsyncOperationHandle<CharacterObject> obj)
    {
    }

    [ContextMenu(nameof(GiveMaggot))]
    public void GiveMaggot()
    {
        StartCoroutine(GiveMaggotRoutine());
    }

    IEnumerator GiveMaggotRoutine()
    {
        if (PlayerSaveManager.Initialized)
        {
            var data = playerDataManager.LoadedData;

            PlayerSave.MaggotState newState = new PlayerSave.MaggotState();

            var ar = RandomMaggot;
            var op = ar.LoadAssetAsync<CharacterObject>();

            yield return op;

            var maggotObject = op.Result;

            TryAddLoadedMaggot(op);

            newState.GUID = ar.AssetGUID;
            newState.Health = maggotObject.GetMaxHealth(1, false);
            newState.Exp = maggotObject.GetExpRequiredForLevel(0, 1);

            playerDataManager.AddNewMaggot(newState);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < LoadedMaggots.Count; i++)
        {
            if (LoadedMaggots[i].IsValid()) Addressables.Release(LoadedMaggots[i]);
        }
    }

    [ContextMenu("RNG Test")]
    public void RNGTest()
    {
        dudCount = 0;
        for (int i = 0; i < 10; i++)
        {
            Debug.Log(RandomRarity);
        }
    }

    public Rarity RandomRarity
    {
        get
        {
            // TODO: Create a pool of cookies that the player owns

            float rng = Random.value;
            // Rolled below rare
            if (rng > chanceOfRare)
            {
                dudCount++;
                if (dudCount == 3)
                    return Rarity.Rare;
                else
                {
                    switch (Random.Range(0, 1))
                    {
                        case 0:
                            return Rarity.Common;
                        case 1:
                            return Rarity.Uncommon;
                    }
                }
            }
            else if (rng > chanceOfSuperRare && rng <= chanceOfRare)
            {
                return Rarity.Rare;
            }
            else if (rng > chanceOfRare && rng <= chanceOfSuperRare)
            {
                return Rarity.SuperRare;
            }

            // If you avoided everything
            return Rarity.UltraRare;
        }
    }

    [ContextMenu("Normalize Weights")]
    void NormalizeValues()
    {
        float weights = chanceOfCommon + chanceOfRare + chanceOfSuperRare;
        chanceOfCommon /= weights;
        chanceOfRare /= weights;
        chanceOfSuperRare /= weights;
    }
}
