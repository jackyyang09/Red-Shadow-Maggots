using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;
using static Facade;

public class MapSceneManager : MonoBehaviour
{
    [SerializeField] DevLocker.Utils.SceneReference battleScene;

    [SerializeField] BattleObject[] battleList;

    [SerializeField] Transform cardStack;
    List<CharacterCardHolder> cardHolders;

    [SerializeField] GameObject cardPrefab;
    [SerializeField] Vector3 cardDelta;

    private void OnEnable()
    {
        MapPlayerTracker.OnEnterNode += OnEnterNode;
    }

    private void OnDisable()
    {
        MapPlayerTracker.OnEnterNode -= OnEnterNode;
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PlayerDataManager.Initialized && BattleStateManager.Initialized);

        cardHolders = new List<CharacterCardHolder>();

        var party = playerDataManager.LoadedData.Party;
        var maggotStates = playerDataManager.LoadedData.MaggotStates;
        for (int i = 0; i < party.Length; i++)
        {
            if (party[i] > -1)
            {
                var co = gachaSystem.GUIDToAssetReference[maggotStates[party[i]].GUID];

                StartCoroutine(gachaSystem.LoadMaggot(co, OnMaggotLoaded));
            }
        }
    }

    void OnMaggotLoaded(CharacterObject obj)
    {
        var card = Instantiate(cardPrefab, cardStack).GetComponent<CharacterCardHolder>();
        card.SetCharacterAndRarity(obj, Rarity.Common);
        cardHolders.Add(card);
        cardHolders.GetLast().transform.localPosition = Vector3.zero + (cardHolders.Count - 1) * cardDelta;
    }

    private void OnEnterNode(NodeType obj)
    {
        switch (obj)
        {
            case NodeType.MinorEnemy:
            case NodeType.EliteEnemy:
            case NodeType.Boss:
                battleStateManager.ResetData();

                var data = battleStateManager.LoadedData;
                var b = battleList[Random.Range(0, battleList.Length)];

                data.RoomLevel = playerDataManager.LoadedData.BattlesFought + 1;
                data.IsBossWave = new bool[b.waves.Count];
                data.UseSpecialCam = new bool[b.waves.Count];

                for (int i = 0; i < b.waves.Count; i++)
                {
                    data.IsBossWave[i] = b.waves[i].IsBossWave;
                    data.UseSpecialCam[i] = b.waves[i].UseSpecialCam;

                    data.EnemyGUIDs.Add(new List<string>());
                    for (int j = 0; j < b.waves[i].Enemies.Length; j++)
                    {
                        string guid = "";
                        if (b.waves[i].Enemies[j] != null)
                        {
                            guid = b.waves[i].Enemies[j].AssetGUID;
                        }
                        data.EnemyGUIDs[i].Add(guid);
                    }
                }
                battleStateManager.SaveData();

                playerDataManager.LoadedData.InBattle = true;
                playerDataManager.SaveData();
                sceneLoader.SwitchScene(battleScene.SceneName);
                break;
            case NodeType.RestSite:
                restNode.Initialize();
                break;
            case NodeType.Store:
                break;
            case NodeType.Treasure:
            case NodeType.Mystery:
                treasureNode.Initialize();
                break;
        }
    }

    public void UpdateCardStack()
    {

    }
}