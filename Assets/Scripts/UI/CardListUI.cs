using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Facade;

public class CardListUI : BasicSingleton<CardListUI>
{
    public enum CardListMode
    {
        PartySetup,
        Upgrade
    }

    [SerializeField] CardListMode mode;

    [SerializeField] float tiltAmount = 10;
    [SerializeField] float scaleAmount = 1.15f;
    [SerializeField] float tweenTime = 0.5f;

    [SerializeField] OptimizedCanvas optimizedCanvas;

    [SerializeField] List<CardCanvasProjection> cardProjections = new List<CardCanvasProjection>();
    bool[] cardLoaded;

    public System.Action OnBackOut;

    private void Start()
    {
        cardLoaded = new bool[cardProjections.Count];
    }

    public void InitializeAsPartySetupUI()
    {
        mode = CardListMode.PartySetup;
        ShowUI();
    }

    public void InitializeAsUpgradeUI()
    {
        mode = CardListMode.Upgrade;
        ShowUI();
    }

    [ContextMenu(nameof(ShowUI))]
    public void ShowUI()
    {
        optimizedCanvas.Show();

        var data = playerDataManager.LoadedData;

        {
            int i = 0;
            for (; i < data.MaggotStates.Count; i++)
            {
                cardProjections[i].Show();
            }

            for (; i < cardProjections.Count; i++)
            {
                cardProjections[i].Hide();
            }
        }

        for (int i = 0; i < data.MaggotStates.Count; i++)
        {
            if (!cardLoaded[i])
            {
                StartCoroutine(LoadCardHolder(i, data.MaggotStates[i].GUID));
            }
        }

        Subscribe();
    }

    public void BackOut()
    {
        OnBackOut?.Invoke();
        HideUI();
    }

    [ContextMenu(nameof(HideUI))]
    public void HideUI()
    {
        optimizedCanvas.Hide();
        Unsubscribe();
    }

    public void Subscribe()
    {
        for (int i = 0; i < cardProjections.Count; i++)
        {
            cardProjections[i].OnCardHovered += OnCardHovered;
            cardProjections[i].OnCardExited += OnCardExited;
            cardProjections[i].OnCardClicked += OnCardClicked;
        }
    }

    public void Unsubscribe()
    {
        for (int i = 0; i < cardProjections.Count; i++)
        {
            cardProjections[i].OnCardHovered -= OnCardHovered;
            cardProjections[i].OnCardExited -= OnCardExited;
            cardProjections[i].OnCardClicked -= OnCardClicked;
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    IEnumerator LoadCardHolder(int index, string GUID)
    {
        var op = gachaSystem.LoadMaggot(gachaSystem.GUIDToAssetReference[GUID]);

        yield return op;

        cardProjections[index].CardHolder.SetCharacterAndRarity(op.Result as CharacterObject, gachaSystem.RandomRarity);
        //cardHolders[index].InitializeStatsCanvas(playerDataManager.LoadedData.MaggotStates[index]);
        //cardHolders[index].StatsCanvas.Show();
        cardLoaded[index] = true;
    }

    private void OnCardHovered(CardCanvasProjection obj)
    {
        var a = obj.transform.localEulerAngles;
        a.z = tiltAmount;
        obj.transform.DOLocalRotate(a, tweenTime, RotateMode.Fast);
        obj.transform.DOScale(scaleAmount, tweenTime);
    }

    private void OnCardExited(CardCanvasProjection obj)
    {
        var a = obj.transform.localEulerAngles;
        a.z = 0;
        obj.transform.DOLocalRotate(a, tweenTime, RotateMode.Fast);
        obj.transform.DOScale(1, tweenTime);
    }

    private void OnCardClicked(CardCanvasProjection obj)
    {
        int index = cardProjections.IndexOf(obj);
        var maggotStates = playerDataManager.LoadedData.MaggotStates;

        var a = obj.transform.localEulerAngles;
        a.x = 0;
        obj.transform.DOLocalRotate(a, tweenTime, RotateMode.Fast);
        obj.transform.DOScale(1, tweenTime);

        switch (mode)
        {
            case CardListMode.PartySetup:
                break;
            case CardListMode.Upgrade:
                //maggotUpgradeUI.InitializeUI(obj, maggotStates[index]);
                maggotUpgradeUI.OptimizedCanvas.Show();
                break;
        }
        
        JSAM.AudioManager.PlaySound(MapMenuSounds.UIClick);
    }
}