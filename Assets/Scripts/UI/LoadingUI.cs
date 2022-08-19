using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static Facade;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] Canvas loadCanvas;
    [SerializeField] RectTransform loadingGraphic;
    [SerializeField] OptimizedCanvas animatedCanvas;
    [SerializeField] Image spinningIcon;
    [SerializeField] TMPro.TextMeshProUGUI text;
    [SerializeField] JSAM.JSAMSoundFileObject startLoadSound;

    private void OnEnable()
    {
        SceneLoader.OnSwitchScene += BeginLoadSequence;
    }

    private void OnDisable()
    {
        SceneLoader.OnSwitchScene -= BeginLoadSequence;
    }

    private void BeginLoadSequence(string sceneName)
    {
        StartCoroutine(LoadRoutine(sceneName));
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        JSAM.AudioManager.PlaySound(startLoadSound);

        animatedCanvas.Show();
        loadingGraphic.anchoredPosition = new Vector2(0, 1080);
        loadingGraphic.DOAnchorPosY(0, 0.5f);
        loadCanvas.enabled = true;
        var tween = spinningIcon.transform.DORotate(new Vector3(0, 0, -360), 1, RotateMode.LocalAxisAdd).SetLoops(-1);

#if UNITY_EDITOR
        yield return new WaitForSeconds(2);
        sceneLoader.LoadSceneOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
#endif

        var op = sceneLoader.LoadSceneOp;
        while (op.progress < 1)
        {
            yield return null;
        }
        
        loadingGraphic.DOAnchorPosY(1080, 0.5f);

        yield return new WaitForSeconds(0.5f);

        tween.Kill();
        animatedCanvas.Hide();
        loadCanvas.enabled = false;
    }
}
