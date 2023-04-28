using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DevLocker.Utils;
using static Facade;

public class VictoryScreenUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI expLabel;
    [SerializeField] SceneReference mapScene;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => BattleStateManager.Initialized);

        expLabel.text = "+" + gameManager.ExpGained.ToString();
    }

    public void ContinueToMap()
    {
        gameManager.ReturnToMapVictorious();
    }
}
