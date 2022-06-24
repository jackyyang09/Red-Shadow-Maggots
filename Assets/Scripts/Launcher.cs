using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class Launcher : MonoBehaviour
{
    [SerializeField, DragAndDropString] string mapScene;
    [SerializeField, DragAndDropString] string battleScene;

    public void StartGame()
    {
        if (!PlayerDataManager.Initialized) return;

        if (playerDataManager.LoadedData.InBattle)
        {
            sceneLoader.SwitchScene(battleScene);
        }
        else
        {
            sceneLoader.SwitchScene(mapScene);
        }
    }
}
