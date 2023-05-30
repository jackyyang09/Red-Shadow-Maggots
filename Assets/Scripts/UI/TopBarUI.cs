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

        UpdateFloorCount(playerDataManager.LoadedData.NodesTravelled);
        UpdateExpCounter(playerDataManager.LoadedData.Exp);
    }

    private void OnEnable()
    {
        PlayerSaveManager.OnUpdateEXP += UpdateExpCounter;
        PlayerSaveManager.OnUpdateFloorCount += UpdateFloorCount;
    }

    private void OnDisable()
    {
        PlayerSaveManager.OnUpdateEXP -= UpdateExpCounter;
        PlayerSaveManager.OnUpdateFloorCount -= UpdateFloorCount;
    }

    private void UpdateExpCounter(int obj)
    {
        expCountLabel.text = obj.ToString();
    }

    private void UpdateFloorCount(int obj)
    {
        floorCountLabel.text = "Floor " + (obj + 1).ToString();
    }
}
