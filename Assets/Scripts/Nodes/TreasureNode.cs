using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Facade;

public class TreasureNode : BasicSingleton<TreasureNode>
{
    [SerializeField] float cardPassDelay = 0.35f;
    [SerializeField] float cardFlipDelay = 0.35f;
    [SerializeField] float cardExamineTime = 0.35f;
    [SerializeField] float cardForce = 10;

    [SerializeField] Transform cardOrigin;
    [SerializeField] Transform[] handTransformTween;
    [SerializeField] List<CharacterCardHolder> cardHolders;
    [SerializeField] Rigidbody[] cardRigidbodies;
    [SerializeField] Cinemachine.CinemachineVirtualCamera vCam;
    [SerializeField] UncrateSequence uncrateSequence;

    PlayerSave.MaggotState[] maggots;

    [ContextMenu("Test")]
    public void Initialize()
    {
        StartCoroutine(TreasureRoutine());
    }

    private void OnEnable()
    {
        for (int i = 0; i < cardHolders.Count; i++)
        {
            cardHolders[i].OnCardClicked += OnCardClicked;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < cardHolders.Count; i++)
        {
            cardHolders[i].OnCardClicked -= OnCardClicked;
        }
    }

    IEnumerator TreasureRoutine()
    {
        vCam.enabled = true;

        var characters = new List<CharacterObject>();
        maggots = new PlayerSave.MaggotState[3];

        for (int i = 0; i < cardHolders.Count; i++)
        {
            cardRigidbodies[i].transform.localPosition = Vector3.zero;

            var maggot = gachaSystem.RandomMaggot;
            var op = gachaSystem.LoadMaggot(maggot);
            yield return op;
            characters.Add(op.Result as CharacterObject);

            cardHolders[i].SetCharacterAndRarity(characters[i], Rarity.Common);

            maggots[i] = new PlayerSave.MaggotState();
            maggots[i].GUID = maggot.AssetGUID;
            int level = playerDataManager.LoadedData.BattlesFought;
            maggots[i].Exp = Mathf.RoundToInt(characters[i].GetExpRequiredForLevel(0, level));
            maggots[i].Health = characters[i].GetMaxHealth(level, false);
        }

        for (int i = 0; i < cardHolders.Count; i++)
        {
            cardRigidbodies[i].transform.localPosition = Vector3.zero;
            cardRigidbodies[i].transform.localEulerAngles = new Vector3(0, -90 + -15 + 15 * i, -90);
            cardRigidbodies[i].isKinematic = false;
            cardRigidbodies[i].velocity = Vector3.zero;
            cardRigidbodies[i].AddForce(-cardRigidbodies[i].transform.up * cardForce, ForceMode.Impulse);

            yield return new WaitForSeconds(cardFlipDelay);

            cardRigidbodies[i].DORotate(new Vector3(0, 180, 0), 0.15f, RotateMode.LocalAxisAdd);

            yield return new WaitForSeconds(cardPassDelay);

            positionBackups[i] = cardRigidbodies[i].transform.position;
        }
    }

    Vector3[] positionBackups = new Vector3[3];
    Vector3[] eulerBackups = new Vector3[3];

    private void OnCardClicked(CharacterCardHolder obj)
    {
        if (!activeCard)
        {
            activeCard = obj;
            StartCoroutine(CardExamineRoutine(obj));
        }
    }

    CharacterCardHolder activeCard;

    IEnumerator CardExamineRoutine(CharacterCardHolder holder)
    {
        int index = cardHolders.IndexOf(holder);

        float tweenTime = cardExamineTime / 2;

        holder.transform.position = positionBackups[index];
        eulerBackups[index] = holder.transform.eulerAngles;

        cardRigidbodies[index].isKinematic = true;

        holder.transform.DOMoveZ(-2.75f, tweenTime);

        yield return new WaitForSeconds(tweenTime);

        holder.transform.position = handTransformTween[0].position;
        holder.transform.DOMove(handTransformTween[1].position, tweenTime);
        holder.transform.eulerAngles = new Vector3(0, 90, 60);

        characterSidebar.UpdateStats(maggots[index], holder.Character);
        characterSidebar.Canvas.Show();
        characterPreview.UpdateCanvasProperties(holder.Character, Rarity.Common);
        characterPreview.OptimizedCanvas.Show();
    }

    [ContextMenu("Choose Maggot")]
    public void ChooseMaggot()
    {
        var playerData = playerDataManager.LoadedData;
        var maggotStates = playerDataManager.LoadedData.MaggotStates;

        var state = maggots[cardHolders.IndexOf(activeCard)];
        playerDataManager.AddNewMaggot(state);
        
        uncrateSequence.UncrateCharacter(activeCard.Character, Rarity.Common, ReturnToMapScreen);
        characterPreview.OptimizedCanvas.Hide();
        ShowcaseSystem.Instance.HideShowcase();
    }

    public void ReturnToMaggotSelection()
    {
        StartCoroutine(CardUnexamineRoutine(activeCard));
    }

    IEnumerator CardUnexamineRoutine(CharacterCardHolder obj)
    {
        int index = cardHolders.IndexOf(obj);

        float tweenTime = cardExamineTime / 2;

        ShowcaseSystem.Instance.HideShowcase();
        characterPreview.OptimizedCanvas.Hide();
        obj.transform.DOMove(handTransformTween[0].position, tweenTime);

        yield return new WaitForSeconds(tweenTime);

        obj.transform.DOMove(positionBackups[index], tweenTime);
        obj.transform.eulerAngles = eulerBackups[index];
        //obj.transform.eulerAngles = new Vector3(0, 90, 60);

        yield return new WaitForSeconds(tweenTime);

        cardRigidbodies[index].isKinematic = false;

        activeCard = null;
    }

    void ReturnToMapScreen()
    {
        StartCoroutine(ReturnToMapRoutine());
    }

    IEnumerator ReturnToMapRoutine()
    {
        for (int i = 0; i < cardHolders.Count; i++)
        {
            if (cardHolders[i] == activeCard)
            {
                activeCard.transform.DOMove(handTransformTween[0].position, cardExamineTime);
            }
            else
            {
                cardHolders[i].transform.DOLocalMove(Vector3.zero, cardExamineTime);
            }
        }

        yield return new WaitForSeconds(cardExamineTime);

        vCam.enabled = false;
        activeCard = null;
    }
}