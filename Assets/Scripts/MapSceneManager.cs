using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;
using static Facade;

public class MapSceneManager : BasicSingleton<MapSceneManager>
{
    [SerializeField] Cinemachine.CinemachineBrain cinemachineBrain;
    public Cinemachine.CinemachineBrain CinemachineBrain => cinemachineBrain;

    [SerializeField] DevLocker.Utils.SceneReference battleScene;

    [SerializeField] BattleObject[] battleList;

    [SerializeField] Transform cardStack;
    List<CharacterCardHolder> cardHolders;

    [SerializeField] GameObject cardPrefab;
    [SerializeField] Vector3 cardDelta;

    public BattleObject NextFightNode;

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
        yield return new WaitUntil(() => PlayerSaveManager.Initialized && BattleStateManager.Initialized);

        cardHolders = new List<CharacterCardHolder>();

        var party = playerDataManager.LoadedData.Party;
        var maggotStates = playerDataManager.LoadedData.MaggotStates;
        for (int i = 0; i < party.Length; i++)
        {
            //if (party[i] > -1)
            //{
            //    var co = gachaSystem.GUIDToAssetReference[maggotStates[party[i]].GUID];
            //
            //    StartCoroutine(gachaSystem.LoadMaggot(co, OnMaggotLoaded));
            //}
        }
    }

    //void OnMaggotLoaded(CharacterObject obj)
    //{
    //    var card = Instantiate(cardPrefab, cardStack).GetComponent<CharacterCardHolder>();
    //    card.SetCharacterAndRarity(obj, Rarity.Common);
    //    cardHolders.Add(card);
    //    cardHolders.GetLast().transform.localPosition = Vector3.zero + (cardHolders.Count - 1) * cardDelta;
    //}

    private void OnEnterNode(NodeType obj)
    {
        switch (obj)
        {
            case NodeType.MinorEnemy:
            case NodeType.EliteEnemy:
            case NodeType.Boss:
                NextFightNode = battleList[Random.Range(0, battleList.Length)];

                battleStateManager.ResetData();

                BattleData.PlayerStates = new List<BattleState.PlayerState>();
                for (int i = 0; i < PlayerData.Party.Length; i++)
                {
                    if (PlayerData.Party[i] == -1) continue;
                    var newState = new BattleState.PlayerState();
                    newState.Index = PlayerData.Party[i];
                    if (PlayerData.MaggotStates[newState.Index] != null)
                    {
                        newState.Health = PlayerData.MaggotStates[newState.Index].Health;
                    }
                    BattleData.PlayerStates.Add(newState);
                }

                BattleData.RoomLevel = playerDataManager.LoadedData.NodesTravelled;
                BattleData.UseSpecialCam = new bool[NextFightNode.waves.Count];

                for (int i = 0; i < NextFightNode.waves.Count; i++)
                {
                    BattleData.UseSpecialCam[i] = NextFightNode.waves[i].UseSpecialCam;

                    BattleData.EnemyGUIDs.Add(new List<string>());
                    for (int j = 0; j < NextFightNode.waves[i].Enemies.Length; j++)
                    {
                        string guid = "";
                        if (NextFightNode.waves[i].Enemies[j] != null)
                        {
                            guid = NextFightNode.waves[i].Enemies[j].AssetGUID;
                        }
                        BattleData.EnemyGUIDs[i].Add(guid);
                    }
                }
                battleStateManager.SaveData();

                partySetup.Initialize();
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

    public void MoveToBattleScene()
    {
        mapScroller.SaveMapPosition();

        playerDataManager.LoadedData.InBattle = true;
        playerDataManager.SaveData();

        mapManager.SaveMap();

        sceneLoader.SwitchScene(battleScene.SceneName);
    }
}