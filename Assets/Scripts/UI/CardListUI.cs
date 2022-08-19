using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Facade;

public class CardListUI : BasicSingleton<CardListUI>
{
    [SerializeField] float tiltAmount = 10;
    [SerializeField] float scaleAmount = 1.15f;
    [SerializeField] float tweenTime = 0.5f;

    [Header("Layout Properties")]
    [SerializeField] int cardsPerRow;
    [SerializeField] Vector2 spacing;
    Vector3 cardOrigin;

    [SerializeField] OptimizedCanvas optimizedCanvas;
    [SerializeField] GameObject firstCard;
    [SerializeField] Transform cardsParent;

    List<CharacterCardHolder> cardHolders;
    List<bool> cardLoaded;

    // Start is called before the first frame update
    void Start()
    {
        cardHolders = new List<CharacterCardHolder>();
        cardLoaded = new List<bool>();

        firstCard.transform.SetParent(cardsParent);
        cardHolders.Add(firstCard.GetComponent<CharacterCardHolder>());
        cardLoaded.Add(false);

        cardOrigin = firstCard.transform.localPosition;
    }

    public void ShowUI()
    {
        cardsParent.gameObject.SetActive(true);
        optimizedCanvas.Show();

        var data = playerDataManager.LoadedData;

        for (int i = 0; i < data.MaggotStates.Count; i++)
        {
            CharacterCardHolder h = null;
            Vector3 pos = new Vector3(cardOrigin.x, cardOrigin.y, cardOrigin.z);
            pos.x += i % cardsPerRow * spacing.x;
            pos.y += i / cardsPerRow * spacing.y;
            if (cardHolders.Count > i)
            {
                h = cardHolders[i];
            }
            else
            {
                h = Instantiate(firstCard, cardsParent).GetComponent<CharacterCardHolder>();
                cardHolders.Add(h);
                cardLoaded.Add(false);
            }
            h.transform.SetParent(cardsParent);
            h.transform.localPosition = pos;
        }

        if (data.MaggotStates.Count < cardHolders.Count)
        {

        }

        for (int i = 0; i < cardHolders.Count; i++)
        {
            cardHolders[i].OnCardHovered += OnCardHovered;
            cardHolders[i].OnCardExited += OnCardExited;
            cardHolders[i].OnCardClicked += OnCardClicked;

            if (!cardLoaded[i])
            {
                StartCoroutine(LoadCardHolder(i, data.MaggotStates[i].GUID));
            }
        }
    }

    public void HideUI()
    {
        cardsParent.gameObject.SetActive(false);
        optimizedCanvas.Hide();

        Unsubscribe();
    }

    public void Subscribe()
    {
        for (int i = 0; i < cardHolders.Count; i++)
        {
            cardHolders[i].OnCardHovered += OnCardHovered;
            cardHolders[i].OnCardExited += OnCardExited;
            cardHolders[i].OnCardClicked += OnCardClicked;
        }
    }

    public void Unsubscribe()
    {
        for (int i = 0; i < cardHolders.Count; i++)
        {
            cardHolders[i].OnCardHovered -= OnCardHovered;
            cardHolders[i].OnCardExited -= OnCardExited;
            cardHolders[i].OnCardClicked -= OnCardClicked;
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

        cardHolders[index].SetCharacterAndRarity(op.Result as CharacterObject, gachaSystem.RandomRarity);
        cardLoaded[index] = true;
    }

    private void OnCardHovered(CharacterCardHolder obj)
    {
        var a = obj.transform.localEulerAngles;
        a.x = tiltAmount;
        obj.transform.DOLocalRotate(a, tweenTime, RotateMode.Fast);
        obj.transform.DOScale(scaleAmount, tweenTime);
    }

    private void OnCardExited(CharacterCardHolder obj)
    {
        var a = obj.transform.localEulerAngles;
        a.x = 0;
        obj.transform.DOLocalRotate(a, tweenTime, RotateMode.Fast);
        obj.transform.DOScale(1, tweenTime);
    }

    private void OnCardClicked(CharacterCardHolder obj)
    {
        int index = cardHolders.IndexOf(obj);
        var maggotStates = playerDataManager.LoadedData.MaggotStates;

        var a = obj.transform.localEulerAngles;
        a.x = 0;
        obj.transform.DOLocalRotate(a, tweenTime, RotateMode.Fast);
        obj.transform.DOScale(1, tweenTime);

        maggotUpgradeUI.InitializeUI(obj, maggotStates[index]);
        maggotUpgradeUI.OptimizedCanvas.Show();

        cardsParent.gameObject.SetActive(false);

        JSAM.AudioManager.PlaySound(MapMenuSounds.UIClick);
    }
}