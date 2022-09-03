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
    [SerializeField] CharacterPreviewUI characterDetails;

    Dictionary<CharacterCardHolder, string> cardHoldersToGUID;

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
        cardHoldersToGUID = new Dictionary<CharacterCardHolder, string>();

        for (int i = 0; i < cardHolders.Count; i++)
        {
            cardRigidbodies[i].transform.localPosition = Vector3.zero;

            var maggot = gachaSystem.RandomMaggot;
            var op = gachaSystem.LoadMaggot(maggot);
            yield return op;
            characters.Add(op.Result as CharacterObject);

            cardHolders[i].SetCharacterAndRarity(characters[i], Rarity.Common);
            cardHoldersToGUID.Add(cardHolders[i], maggot.AssetGUID);
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

    IEnumerator CardExamineRoutine(CharacterCardHolder obj)
    {
        int index = cardHolders.IndexOf(obj);

        float tweenTime = cardExamineTime / 2;

        obj.transform.position = positionBackups[index];
        eulerBackups[index] = obj.transform.eulerAngles;

        cardRigidbodies[index].isKinematic = true;

        obj.transform.DOMoveZ(-2.75f, tweenTime);

        yield return new WaitForSeconds(tweenTime);

        obj.transform.position = handTransformTween[0].position;
        obj.transform.DOMove(handTransformTween[1].position, tweenTime);
        obj.transform.eulerAngles = new Vector3(0, 90, 60);
        characterDetails.OptimizedCanvas.Show();
        characterDetails.UpdateCanvasProperties(obj.Character, Rarity.Common);
    }

    [ContextMenu("Choose Maggot")]
    public void ChooseMaggot()
    {
        var playerData = playerDataManager.LoadedData;
        var maggotStates = playerDataManager.LoadedData.MaggotStates;
        PlayerData.MaggotState newState = new PlayerData.MaggotState();
        newState.GUID = cardHoldersToGUID[activeCard];
        int level = 1;
        newState.Exp = Mathf.RoundToInt(activeCard.Character.GetExpRequiredForLevel(0, level));
        newState.Health = activeCard.Character.GetMaxHealth(level, false);

        for (int i = 0; i < playerData.Party.Length; i++)
        {
            if (playerData.Party[i] == -1)
            {
                // This is okay since we add to maggot states immediately after
                playerData.Party[i] = maggotStates.Count;
                break;
            }
        }

        playerDataManager.AddNewMaggot(newState);
        playerDataManager.SaveData();
        
        uncrateSequence.UncrateCharacter(activeCard.Character, Rarity.Common, ReturnToMapScreen);
        characterDetails.OptimizedCanvas.Hide();
    }

    public void ReturnToMaggotSelection()
    {
        StartCoroutine(CardUnexamineRoutine(activeCard));
    }

    IEnumerator CardUnexamineRoutine(CharacterCardHolder obj)
    {
        int index = cardHolders.IndexOf(obj);

        float tweenTime = cardExamineTime / 2;

        characterDetails.OptimizedCanvas.Hide();
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