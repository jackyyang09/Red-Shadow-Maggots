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
    List<AsyncOperationHandle> LoadedMaggots = new List<AsyncOperationHandle>();
    List<AssetReference> loadingMaggots = new List<AssetReference>();
    Dictionary<string, AssetReference> guidToAssetRef = new Dictionary<string, AssetReference>();
    public Dictionary<string, AssetReference> GUIDToAssetReference { get { return guidToAssetRef; } }

    [SerializeField] List<CharacterObject> maggots = null;

    [Header("Legacy")]
    List<CharacterObject> offenseMaggots = new List<CharacterObject>();
    List<CharacterObject> defenseMaggots = new List<CharacterObject>();
    List<CharacterObject> supportMaggots = new List<CharacterObject>();

    private void Awake()
    {
        LoadedMaggots = new List<AsyncOperationHandle>();
        loadingMaggots = new List<AssetReference>();

        for (int i = 0; i < maggotReferences.Count; i++)
        {
            guidToAssetRef.Add(maggotReferences[i].AssetGUID, maggotReferences[i]);
        }
    }

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

    public AsyncOperationHandle LoadMaggot(AssetReference reference)
    {
        if (LoadedMaggots.Contains(reference.OperationHandle)) return reference.OperationHandle;

        loadingMaggots.Add(reference);
        var op = reference.LoadAssetAsync<CharacterObject>();
        LoadedMaggots.Add(op);
        loadingMaggots.Remove(reference);
        op.Completed += OnCharacterLoaded;
        return op;
    }

    private void OnCharacterLoaded(AsyncOperationHandle<CharacterObject> obj)
    {
    }

    public IEnumerator LoadMaggot(AssetReference reference, System.Action<CharacterObject> callback)
    {
        if (!LoadedMaggots.Contains(reference.OperationHandle) && !loadingMaggots.Contains(reference))
        {
            loadingMaggots.Add(reference);

            var op = reference.LoadAssetAsync<CharacterObject>();

            yield return op;

            LoadedMaggots.Add(op);

            loadingMaggots.Remove(reference);

            callback?.Invoke(op.Result);
        }
        else
        {
            while (!reference.IsDone)
            {
                yield return new WaitUntil(() => reference.IsDone);
            }
            callback?.Invoke(reference.OperationHandle.Result as CharacterObject);
        }
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
            var op = LoadMaggot(ar);

            yield return op;

            var maggotObject = op.Result as CharacterObject;

            newState.GUID = ar.AssetGUID;
            newState.Health = maggotObject.GetMaxHealth(1, false);
            newState.Exp = maggotObject.GetExpRequiredForLevel(0, 1);

            playerDataManager.AddNewMaggot(newState);
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
