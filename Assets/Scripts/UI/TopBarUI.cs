using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Facade;
using System;

public class TopBarUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI expCountLabel;
    [SerializeField] TextMeshProUGUI floorCountLabel;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PlayerSaveManager.Initialized);

        UpdateFloorcount(playerDataManager.LoadedData.NodesTravelled);
        UpdateExpCounter(playerDataManager.LoadedData.Exp);
    }

    private void OnEnable()
    {
        PlayerSaveManager.OnUpdateEXP += UpdateExpCounter;
        PlayerSaveManager.OnUpdateFloorCount += UpdateFloorcount;
    }

    private void OnDisable()
    {
        PlayerSaveManager.OnUpdateEXP -= UpdateExpCounter;
        PlayerSaveManager.OnUpdateFloorCount -= UpdateFloorcount;
    }

    private void UpdateExpCounter(int obj)
    {
        expCountLabel.text = obj.ToString();
    }

    private void UpdateFloorcount(int obj)
    {
        floorCountLabel.text = "Floor " + obj.ToString();
    }
}
