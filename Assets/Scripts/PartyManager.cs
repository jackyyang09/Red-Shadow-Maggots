using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;
using static Facade;

public class PartyManager : BasicSingleton<PartyManager>
{
    [SerializeField] Transform cardStack;
    List<CharacterCardHolder> cardHolders;

    [SerializeField] GameObject cardPrefab;
    [SerializeField] Vector3 cardDelta;

    bool mousedOver;

    private void OnEnable()
    {
        PlayerSaveManager.OnMaggotStatesChanged += UpdateCardStack;
    }

    private void OnDisable()
    {
        PlayerSaveManager.OnMaggotStatesChanged -= UpdateCardStack;
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PlayerSaveManager.Initialized && BattleStateManager.Initialized);

        InstantiateCardHolders();
    }

    void InstantiateCardHolders()
    {
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
    //    card.OnCardHovered += MousedOver;
    //    card.OnCardExited += MousedAway;
    //    card.OnCardClicked += MouseClicked;
    //    cardHolders.Add(card);
    //    cardHolders.GetLast().transform.localPosition = Vector3.zero + (cardHolders.Count - 1) * cardDelta;
    //}

    void MousedOver(CharacterCardHolder c)
    {

    }

    void MousedAway(CharacterCardHolder c)
    {

    }

    private void MouseClicked(CharacterCardHolder obj)
    {
        cardListUI.InitializeAsPartySetupUI();
    }

    public void UpdateCardStack()
    {
        for (int i = cardStack.childCount - 1; i > -1 && cardStack.childCount > 0; i--)
        {
            Destroy(cardStack.GetChild(i).gameObject);
        }

        InstantiateCardHolders();
    }
}