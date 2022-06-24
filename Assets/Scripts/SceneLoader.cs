using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    public AsyncOperation LoadSceneOp;

    public static System.Action<string> OnSwitchScene;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
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

    public void SwitchScene(string scene)
    {
        StartCoroutine(SwitchSceneRoutine(scene));
    }

    IEnumerator SwitchSceneRoutine(string scene)
    {
#if !UNITY_EDITOR
        LoadSceneOp = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
#endif
        OnSwitchScene?.Invoke(scene);
        yield return LoadSceneOp;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
