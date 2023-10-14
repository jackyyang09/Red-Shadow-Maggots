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

    [SerializeField] BattleObject bossBattle;

    [SerializeField] Transform cardStack;
    List<CharacterCardHolder> cardHolders;

    [SerializeField] GameObject cardPrefab;
    [SerializeField] Vector3 cardDelta;

    BattleObject nextBattleNode;
    public BattleObject NextFightNode
    {
        get => nextBattleNode;
    }

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
                Random.InitState(PlayerData.NodesTravelled);
                nextBattleNode = battleList[Random.Range(0, battleList.Length)];
                partySetup.Initialize();
                break;
            case NodeType.Boss:
                nextBattleNode = bossBattle;
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