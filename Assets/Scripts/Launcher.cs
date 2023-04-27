using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class Launcher : MonoBehaviour
{
    [SerializeField] DevLocker.Utils.SceneReference mapScene;
    [SerializeField] DevLocker.Utils.SceneReference battleScene;

    public void StartGame()
    {
        if (!PlayerSaveManager.Initialized) return;

        if (playerDataManager.LoadedData.InBattle)
        {
            sceneLoader.SwitchScene(battleScene.SceneName);
        }
        else
        {
            sceneLoader.SwitchScene(mapScene.SceneName);
        }
    }
}
