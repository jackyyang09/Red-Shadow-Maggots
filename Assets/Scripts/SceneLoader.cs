using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDisable()
    {
        
    }

    private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
    {
        readyToLoadScene = true;
    }

    bool readyToLoadScene = true;

    public void ReloadScene()
    {
        if (!readyToLoadScene) return;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
        readyToLoadScene = false;
    }

    public void SwitchScene(int scene)
    {
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
    }
}
